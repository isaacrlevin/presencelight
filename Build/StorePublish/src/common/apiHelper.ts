/*
 * A helper for the Store API. Contains logic to obtain App IDs from names, poll submissions,
 * create files required by the API, etc.
 */

/// <reference path="../../typings/index.d.ts" />

import request = require("./requestHelper");

import fs = require("fs");
import path = require("path");

var JSZip = require("jszip"); // JSZip typings have not been updated to the version we're using
import Q = require("q");

import {
  AnonymousCredential,
  BlobServiceClient,
  newPipeline,
} from "@azure/storage-blob";
var url = require("url");

/**
 * A little part of the URL to the API that contains a version number.
 * This may need to be updated in the future to comply with the API.
 */
export const API_URL_VERSION_PART = "/v1.0/my/";

/** The root of all API requests. */
export var ROOT: string;

/** How many times should we retry. */
export const NUM_RETRIES = 5;

/** Maximum number of packages allowed in one flight group */
export const MAX_PACKAGES_PER_GROUP = 25;

/**
 * The message used when a commit fails. Note that this does not need to be very
 * informative since the user will see more details in additional messages.
 */
const COMMIT_FAILED_MSG = "Commit failed";

export function includePackagesInSubmission(
  givenPackages: string[],
  submissionPackages: { fileName: string; fileStatus: string }[]
): void {
  console.debug(`Adding ${givenPackages.length} package(s)`);
  givenPackages.map(makePackageEntry).forEach((packEntry) => {
    var entry = {
      fileName: packEntry,
      fileStatus: "PendingUpload",
    };

    submissionPackages.push(entry);
  });
}

/**
 * Create a zip file containing the given list of packages.
 * @param submissionResource
 */
export function createZipFromPackages(packages: string[]) {
  console.debug(`Creating zip file`);
  var zip = new JSZip();

  packages.forEach((aPath, i) => {
    // According to JSZip documentation, the directory separator used is a forward slash.
    var entry = makePackageEntry(aPath, i).replace(/\\/g, "/");
    console.debug(`Adding package path ${aPath} to zip as ${entry}`);
    zip.file(entry, fs.createReadStream(aPath), { compression: "DEFLATE" });
  });

  return zip;
}

/**
 * Polls the status of a submission
 * @param submissionResource The submission to poll
 * @return A promise that will be fulfilled if the commit is successful, and rejected if the commit fails.
 */
export function pollSubmissionStatus(
  token: request.AccessToken,
  resourceLocation: string,
  publishMode: string
): Q.Promise<void> {
  const POLL_DELAY = 300000; // Delay 5 minutes between poll requests
  var submissionCheckGenerator = () =>
    checkSubmissionStatus(token, resourceLocation, publishMode);
  return request
    .withRetry(
      NUM_RETRIES,
      submissionCheckGenerator,
      (err) =>
        // Keep trying unless it's a non-retriable error or the message is the one we use for failed commits.
        request.isRetryableError(err) &&
        !(err != undefined && err.message == COMMIT_FAILED_MSG)
    )
    .then((status) => {
      if (status) {
        return;
      } else {
        return Q.delay(POLL_DELAY)
          .then(() =>
            pollSubmissionStatus(token, resourceLocation, publishMode)
          )
          .catch((err) => {
            console.log(err);
            throw err;
          });
      }
    });
}

/**
 * Checks the status of a submission.
 * @param submissionId
 * @return A promise for the status of the submission: true for completed, false for not completed yet.
 * The promise will be rejected if an error occurs in the submission.
 */
function checkSubmissionStatus(
  token: request.AccessToken,
  resourceLocation: string,
  publishMode: string
): Q.Promise<boolean> {
  const statusMsg = `Submission status for "${resourceLocation}"`;
  const requestParams = {
    url: ROOT + resourceLocation + "/status",
    method: "GET",
  };

  return request
    .performAuthenticatedRequest<any>(token, requestParams)
    .then(function (body) {
      /* Once the previous request has finished, examine the body to tell if we should start a new one. */
      if (!body.status.endsWith("Failed")) {
        var msg = statusMsg + body.status;
        console.debug(statusMsg + body.status);
        console.log(msg);

        /* In immediate mode, we expect to get all the way to "Published" status.
         * In other modes, we stop at "Release" status. */
        return (
          body.status == "Published" ||
          (body.status == "Release" && publishMode != "Immediate")
        );
      } else {
        console.error(statusMsg + " failed with " + body.status);
        console.error("Reported errors: ");
        for (var i = 0; i < body.statusDetails.errors.length; i++) {
          var errDetail = body.statusDetails.errors[i];
          console.error("\t " + errDetail.code + ": " + errDetail.details);
        }
        throw new Error(COMMIT_FAILED_MSG);
      }
    })
    .catch((err) => {
      console.log(err);
      throw new Error(COMMIT_FAILED_MSG);
    });
}

/**
 * @return Promises the resource associated with the application given to the task.
 */
export function getAppResource(
  token: request.AccessToken,
  appId: string
): Q.Promise<any> {
  console.debug(`Getting app resource from ID ${appId}`);
  var requestParams = {
    url: ROOT + "applications/" + appId,
    method: "GET",
  };

  var getGenerator = () =>
    request.performAuthenticatedRequest<any>(token, requestParams);
  return request.withRetry(NUM_RETRIES, getGenerator, (err) =>
    request.isRetryableError(err)
  );
}

/**
 * @return Promises the deletion of a resource
 */
export function deleteSubmission(
  token: request.AccessToken,
  url: string
): Q.Promise<void> {
  console.debug(`Deleting submission at ${url}`);
  var requestParams = {
    url: url,
    method: "DELETE",
  };

  var deleteGenerator = () =>
    request.performAuthenticatedRequest<void>(token, requestParams);
  return request.withRetry(NUM_RETRIES, deleteGenerator, (err) =>
    request.isRetryableError(err, false)
  );
}

/**
 * Creates a submission for a given app.
 * @return Promises the new submission resource.
 */
export function createSubmission(
  token: request.AccessToken,
  url: string
): Q.Promise<any> {
  console.debug("Creating new submission");
  var requestParams = {
    url: url,
    method: "POST",
  };

  var putGenerator = () =>
    request.performAuthenticatedRequest<any>(token, requestParams);
  return request.withRetry(NUM_RETRIES, putGenerator, (err) =>
    request.isRetryableError(err, false)
  );
}

/**
 * Updates a submission for a given app.
 * @returns A promise for the update of the submission on the server.
 */
export function putSubmission(
  token: request.AccessToken,
  url: string,
  submissionResource: any
): Q.Promise<void> {
  console.debug(`Updating submission`);

  var requestParams = {
    url: url,
    method: "PUT",
    json: true, // Sets content-type and length for us, and parses the request/response appropriately
    body: submissionResource,
  };

  var putGenerator = () =>
    request.performAuthenticatedRequest<void>(token, requestParams);
  return request.withRetry(NUM_RETRIES, putGenerator, (err) =>
    request.isRetryableError(err, false)
  );
}

/**
 * Commits a submission, checking for any errors.
 * @return A promise for the commit of the submission
 */
export function commitSubmission(
  token: request.AccessToken,
  url: string
): Q.Promise<void> {
  console.debug(`Committing submission`);

  var requestParams = {
    url: url,
    method: "POST",
  };

  var postGenerator = () =>
    request.performAuthenticatedRequest<void>(token, requestParams);
  return request.withRetry(NUM_RETRIES, postGenerator, (err) =>
    request.isRetryableError(err, false)
  );
}

/**
 * Updates submission's package delivery options.
 * @param submissionResource The current submission request
 * @param mandatoryUpdateDifferHours The number of hours to differ until the packages in this submission become mandatory
 * @returns A promise for the update of the submission on the server.
 */
export function updatePackageDeliveryOptions(
  submissionResource: any,
  mandatoryUpdateDifferHours?: number
): void {
  var mandatoryUpdateEffectiveDate: Date = new Date(0);
  if (mandatoryUpdateDifferHours) {
    mandatoryUpdateEffectiveDate.setTime(
      Date.now() + mandatoryUpdateDifferHours * 60 * 60 * 1000
    );
    console.debug(
      `Setting isMandatoryUpdate to true, mandatoryUpdateEffectiveDate to ${mandatoryUpdateEffectiveDate.toISOString()}`
    );
  } else {
    console.debug(`Setting isMandatoryUpdate to false`);
  }

  submissionResource.packageDeliveryOptions.isMandatoryUpdate =
    mandatoryUpdateDifferHours !== null;
  submissionResource.packageDeliveryOptions.mandatoryUpdateEffectiveDate = mandatoryUpdateEffectiveDate.toISOString();
}

/**
 * Creates a buffer to the given zip file.
 */
function createZipStream(zip: {
  generateNodeStream: (arg0: {
    base64: boolean;
    compression: string;
    type: string;
    streamFiles: boolean;
  }) => NodeJS.ReadableStream;
}): NodeJS.ReadableStream {
  var zipGenerationOptions = {
    base64: false,
    compression: "DEFLATE",
    type: "nodebuffer",
    streamFiles: true,
  };

  return zip.generateNodeStream(zipGenerationOptions);
}

/**
 * Creates a zip file using given zip stream.
 */
function createZipFile(
  zipStream: NodeJS.ReadableStream,
  filename: string
): Q.Promise<string> {
  var defer = Q.defer<string>();

  zipStream
    .pipe(fs.createWriteStream(filename))
    .on("finish", function () {
      defer.resolve();
    })
    .on("error", function (err) {
      defer.reject(`Failed to create ${filename}. Error = ${err}`);
    });

  return defer.promise;
}

/**
 * Write the given zip file to disk and to the given Azure blob.
 * @param zip
 * @param filePath
 * @param blobUrl
 */
export async function persistZip(
  zip: {
    generateNodeStream: (arg0: {
      base64: boolean;
      compression: string;
      type: string;
      streamFiles: boolean;
    }) => NodeJS.ReadableStream;
  },
  filePath: string,
  blobUrl: string
) {
  var buf: NodeJS.ReadableStream = createZipStream(zip);
  await createZipFile(buf, filePath);
  console.log("Uploading zip file...");
  return await uploadZip(filePath, blobUrl);
}

/**
 * Uploads a zip file to the appropriate blob.
 * @param filePath Path of the zip file to upload
 * @return A promise for the upload of the zip file.
 */
async function uploadZip(filePath: string, blobUrl: string): Promise<any> {
  console.debug(`Uploading zip file to ${blobUrl}`);
  /* The URL we get from the Store sometimes has unencoded '+' and '=' characters because of a
   * base64 parameter. There is no good way to fix this, because we don't really know how to
   * distinguish between 'correct' uses of those characters, and their spurious instances in
   * the base64 parameter. In our case, we just take the compromise of replacing every instance
   * of '+' with its url-encoded counterpart. */
  var dest = blobUrl.replace(/\+/g, "%2B");

  var urlObject = url.parse(dest);

  var pathParts = urlObject.pathname.split("/");
  //pathname property returns path with leading '/'. Thus, pathParts[0] will always be empty.
  var containerName = pathParts[1];
  var blobName = pathParts[2];

  var host = urlObject.host;
  var sasToken = urlObject.search;

  const pipeline = newPipeline(new AnonymousCredential(), {
    retryOptions: { maxTries: 4 }, // Retry options
    userAgentOptions: { userAgentPrefix: "AdvancedSample V1.0.0" }, // Customized telemetry string
    keepAliveOptions: {
      // Keep alive is enabled by default, disable keep alive by setting false
      enable: false,
    },
  });

  const blobServiceClient = new BlobServiceClient(
    `https://${host}${sasToken}`,
    pipeline
  );

  const containerClient = blobServiceClient.getContainerClient(containerName);
  const blockBlobClient = containerClient.getBlockBlobClient(blobName);
  var defer = Q.defer();
  try {
    await blockBlobClient.uploadFile(filePath, {
      blockSize: 4 * 1024 * 1024, // 4MB block size
      concurrency: 20,
    });
    console.log("uploadFile succeeds");
    defer.resolve();
  } catch (err) {
    console.log(
      `uploadFile failed, requestId - ${err.details.requestId}, statusCode - ${err.statusCode}, errorCode - ${err.details.errorCode}`
    );
    defer.reject(`Failed to upload file! Error = ${err}`);
  }
  return defer.promise;
}

/**
 * Transform a package path into a package entry for the zip file.
 * All leading directories are removed and replaced by the index. E.g.
 *
 * ['foo/anAppx', 'bar/anAppx', 'baz/quux/aXap'].map(makePackageEntry)
 *      => ['0/anAppx', '1/anAppx', '2/aXap']
 */
function makePackageEntry(pack: string, i: number): string {
  return i.toString() + "_" + path.basename(pack);
}
