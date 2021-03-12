require("dotenv").config();

/// <reference path="../../typings/index.d.ts" />
import fs = require('fs');
import path = require('path');
import api = require("../common/apiHelper");
import request = require("../common/requestHelper");
import Q = require("q");

var skipPolling = true;
var packages: any[] = [];
/** The current token used for authentication. */
var currentToken: request.AccessToken;

/** The app ID we are publishing to */
var appId: string;

/**
 * The main task function.
 */
export async function publishTask() {
  /* We expect the endpoint part of this to not have a slash at the end.
   * This is because authenticating to 'endpoint/' will give us an
   * invalid token, while authenticating to 'endpoint' will work */
  api.ROOT = process.env.ENDPOINT + api.API_URL_VERSION_PART;

  var credentials = {
    tenant: process.env.TENANT,
    clientId: process.env.CLIENT_ID,
    clientSecret: process.env.CLIENT_SECRET,
  };
  packages.push(process.env.PACKAGE_PATH);
  console.log("Authenticating...");
  currentToken = await request.authenticate(process.env.ENDPOINT, credentials);
  appId = process.env.APP_ID; // Globally set app ID for future steps.

  console.log("Creating submission...");
  var submissionResource = await createAppSubmission();
  var submissionUrl = `https://developer.microsoft.com/en-us/dashboard/apps/${appId}/submissions/${submissionResource.id}`;
  console.log(`Submission ${submissionUrl} was created successfully`);

  console.log("Deleting old packages...");
  api.deleteOldPackages(submissionResource.applicationPackages, 0);

  console.log("Updating package delivery options...");
  await api.updatePackageDeliveryOptions(submissionResource, 0);

  console.log("Updating submission...");
  await putMetadata(submissionResource);

  console.log("Creating zip file...");


  var zip = api.createZipFromPackages(packages);
  addImagesToZip(submissionResource, zip);

  // There might be no files in the zip if the user didn't supply any packages or images.
  // If there are files, persist the file.
  if (Object.keys(zip.files).length > 0) {
    await api.persistZip(zip, "temp.zip", submissionResource.fileUploadUrl);
  }

  console.log("Committing submission...");
  await commitAppSubmission(submissionResource.id);

  if (skipPolling) {
    console.log("Skip polling option is checked. Skipping polling...");
    console.log(
      `Click here ${submissionUrl} to check the status of the submission in Dev Center`
    );
  } else {
    console.log("Polling submission...");
    var resourceLocation = `applications/${appId}/submissions/${submissionResource.id}`;
    await api.pollSubmissionStatus(
      currentToken,
      resourceLocation,
      submissionResource.targetPublishMode
    );
  }

  console.log("Submission completed");
}

/**
 * Creates a submission for a given app.
 * @return Promises the new submission resource.
 */
function createAppSubmission(): Q.Promise<any> {
  return api.createSubmission(
    currentToken,
    api.ROOT + "applications/" + appId + "/submissions"
  );
}

/**
 * Add any PendingUpload images in the given submission resource to the given zip file.
 */
function addImagesToZip(
  submissionResource: any,
  zip: { file: (arg0: any, arg1: any, arg2: { compression: string }) => void }
) {
  for (var listingKey in submissionResource.listings) {
    console.log(`Checking for new images in listing ${listingKey}...`);
    var listing = submissionResource.listings[listingKey];

    if (listing.baseListing.images) {
      addImagesToZipFromListing(listing.baseListing.images, zip);
    }

    for (var platOverrideKey in listing.platformOverrides) {
      console.log(
        `Checking for new images in platform override ${listingKey}/${platOverrideKey}...`
      );
      var platOverride = listing.platformOverrides[platOverrideKey];

      if (platOverride.images) {
        addImagesToZipFromListing(platOverride.images, zip);
      }
    }
  }
}

function addImagesToZipFromListing(
  images: any[],
  zip: { file: (arg0: any, arg1: any, arg2: { compression: string }) => void }
) {
  images
    .filter((image) => image.fileStatus == "PendingUpload")
    .forEach((image) => {
      var imgPath = path.join("", image.fileName);
      // According to JSZip documentation, the directory separator used is a forward slash.
      var filenameInZip = image.fileName.replace(/\\/g, "/");
      console.log(`Adding image path ${imgPath} to zip as ${filenameInZip}`);
      zip.file(filenameInZip, fs.createReadStream(imgPath), {
        compression: "DEFLATE",
      });
    });
}

/**
 * Commits a submission, checking for any errors.
 * @return A promise for the commit of the submission
 */
function commitAppSubmission(submissionId: string): Q.Promise<void> {
  return api.commitSubmission(
    currentToken,
    api.ROOT +
      "applications/" +
      appId +
      "/submissions/" +
      submissionId +
      "/commit"
  );
}

/**
 * Adds the required metadata to the submission request, depending on the given parameters.
 * If no metadata update is to be perfomed, no changes are made. Otherwise, we look for the metadata
 * depending on the type of update (text or json).
 * @param submissionResource The current submission request
 * @returns A promise for the update of the submission on the server.
 */
function putMetadata(submissionResource: any): Q.Promise<void> {
  console.log(`Adding metadata for new submission ${submissionResource.id}`);

  // Also at this point add the given packages to the list of packages to upload.
  api.includePackagesInSubmission(
    packages,
    submissionResource.applicationPackages
  );

  var url =
    api.ROOT +
    "applications/" +
    appId +
    "/submissions/" +
    submissionResource.id;

  return api.putSubmission(currentToken, url, submissionResource);
}

async function main() {
  await publishTask();
}

main();
