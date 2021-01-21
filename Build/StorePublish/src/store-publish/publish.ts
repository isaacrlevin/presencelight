require("dotenv").config();

/// <reference path="../../typings/index.d.ts" />

import api = require("../common/apiHelper");
import request = require("../common/requestHelper");
import Q = require("q");

var skipPolling = true;

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

  console.log("Authenticating...");
  currentToken = await request.authenticate(process.env.ENDPOINT, credentials);
  appId = process.env.APP_ID; // Globally set app ID for future steps.

  console.log("Creating submission...");
  var submissionResource = await createAppSubmission();
  var submissionUrl = `https://developer.microsoft.com/en-us/dashboard/apps/${appId}/submissions/${submissionResource.id}`;
  console.log(`Submission ${submissionUrl} was created successfully`);

  console.log("Creating zip file...");

  var packages = [];
  packages.push(process.env.PACKAGE_PATH);

  var zip = api.createZipFromPackages(packages);

  // There might be no files in the zip if the user didn't supply any packages or images.
  // If there are files, persist the file.
  if (Object.keys(zip.files).length > 0) {
    await api.persistZip(
      zip,
      "temp.zip",
      submissionResource.fileUploadUrl
    );
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

async function main() {
  await publishTask();
}

main();
