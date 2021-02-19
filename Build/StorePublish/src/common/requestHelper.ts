/*
 * A helper to perform various HTTP requests, with some default handling to manage errors.
 * This is mainly a wrapper for the 'request' npm module that uses promises instead of callbacks.
 */

/// <reference path="../../typings/index.d.ts" />

import http = require('http'); // Only used for types

import { v4 as uuidv4 } from 'uuid'
import Q = require('q');
import request = require('request');

/** How long to wait between retries (in ms) */
const RETRY_DELAY = 60000;

/** After how long should a connection be given up (in ms). */
const TIMEOUT = 600000;

/** Credentials used to gain access to a particular resource. */
export interface Credentials
{
    /** The tenant ID associated with these credentials. */
    tenant: string;
    clientId: string;
    clientSecret: string;
}

/** A token used to access a particular resource. */
export interface AccessToken
{
    /** Resource to which this token grants access. */
    resource: string

    /** Credentials used to obtain access. */
    credentials: Credentials;

    /** Expiration timestamp of the token */
    expiration: number;

    /** Actual token to be used in the request. */
    token: string;
}

/** Whether an access token should be renewed. */
function isExpired(token: AccessToken): boolean
{
    // Date.now() returns a number in miliseconds.
    // We say that a token is expired if its expiration date is at most five seconds in the future.
    return (Date.now() / 1000) + 5 > token.expiration;
}

/** All the information given to us by the request module along a response. */
export class ResponseInformation
{
    error: any;
    response: http.IncomingMessage;
    body: any;

    constructor(_err: any, _res: http.IncomingMessage, _bod: any)
    {
        this.error = _err;
        this.response = _res;
        this.body = _bod;
    }

    // For friendly logging
    toString(): string
    {
        var log: string;

        if (this.error != undefined)
        {
            log = `Error ${JSON.stringify(this.error)}`;
        }
        else
        {
            var bodyToPrint = this.body;
            if (typeof bodyToPrint != 'string')
            {
                bodyToPrint = JSON.stringify(bodyToPrint);
            }
            var statusCode: string = (this.response != undefined && this.response.statusCode != undefined) ? this.response.statusCode.toString() : 'unknown';
            log = `Status ${statusCode}: ${bodyToPrint}`;
        }

        if (this.response != undefined &&
            this.response.headers['ms-correlationid'] != undefined)
        {
            log = log + ` CorrelationId: ${this.response.headers['ms-correlationid']}`;
        }

        return log;
    }
}

/**
 * Perform a request with some default handling.
 *
 * For convenience, parses the body if the content-type is 'application/json'.
 * Further, examines the body and logs any errors or warnings.
 *
 * If an transport or application level error occurs, rejects the returned promise.
 * The reason given is an instance of @ResponseInformation@, containing the error
 * object, the response and the body.
 *
 * If no error occurs, resolves the returned promise with the body.
 *
 * @param options Options describing the request to execute.
 * @param stream If specified, pipe this stream into the request.
 */
export function performRequest<T>(
    options: (request.UriOptions | request.UrlOptions) & request.CoreOptions,
    stream?: NodeJS.ReadableStream):
    Q.Promise<T>
{
    var deferred = Q.defer<T>();

    if (options.timeout == undefined)
    {
        options.timeout = TIMEOUT;
    }

    // Log correlation Id for better diagnosis
    var correlationId = uuidv4();
    console.debug(`Starting request with correlation id: ${correlationId}`);
    if (options.headers === undefined)
    {
        options.headers = {
            'CorrelationId': correlationId
        }
    }
    else
    {
        options.headers['CorrelationId'] = correlationId;
    }

    var callback = function (error: any, response: any, body: any)
    {
        // For convenience, parse the body if it's JSON.
        if (response != undefined && // response is undefined if a transport-level error occurs
            response.headers['content-type'] != undefined &&   // content-type is undefined if there is no content
            response.headers['content-type'].indexOf('application/json') != -1 &&
            typeof body == 'string') // body might be an object if the options given to request already parsed it for us
        {
            body = JSON.parse(body);
            logErrorsAndWarnings(response, body);
        }

        if (error || (response && response.statusCode != undefined && response.statusCode >= 400))
        {
            deferred.reject(new ResponseInformation(error, response, body));
        }
        else
        {
            deferred.resolve(body);
        }
        console.debug(`Finished request with correlation id: ${correlationId}`);
    }

    if (!stream)
    {
        request(options, callback);
    }
    else
    {
        stream.pipe(request(options, callback));
    }

    return deferred.promise;
}

/**
 * Same as @performRequest@, but additionally requires an authentification token.
 * @param auth A token used to identify with the resource. If expired, it will be renewed before executing the request.
 */
export function performAuthenticatedRequest<T>(
    auth: AccessToken,
    options: (request.UriOptions | request.UrlOptions) & request.CoreOptions):
    Q.Promise<T>
{
    // The expiration check is a function that returns a promise
    var expirationCheck = function ()
    {
        if (isExpired(auth))
        {
            return authenticate(auth.resource, auth.credentials).then(function (newAuth: { token: string; expiration: number; })
            {
                auth.token = newAuth.token;
                auth.expiration = newAuth.expiration;
            }).catch(err => {
                console.log(err) ;
                throw err;
            });
        }
        else
        {
            /* This looks strange, but it returns a promise for void, which is exactly what we need. */
            return Q.when();
        }
    };


    return expirationCheck() // Call the expiration check to obtain a promise for it.
        .then<T>(function () // Chain the use of the token to that promise.
        {
            if (options.headers === undefined)
            {
                options.headers = {
                    'Authorization': 'Bearer ' + auth.token
                }
            }
            else
            {
                options.headers['Authorization'] = 'Bearer ' + auth.token;
            }

            return performRequest<T>(options);
        }).catch(err => {
            console.log(err)
            throw err;
        });
}

/**
 * @param resource The resource (URL) to authenticate to.
 * @param credentials Credentials to use for authentication.
 * @returns Promises an access token to use to communicate with the resource.
 */
export function authenticate(resource: string, credentials: Credentials): Q.Promise<AccessToken>
{
    var endpoint = 'https://login.microsoftonline.com/' + credentials.tenant + '/oauth2/token';
    var requestParams = {
        grant_type: 'client_credentials',
        client_id: credentials.clientId,
        client_secret: credentials.clientSecret,
        resource: resource
    };

    var options = {
        url: endpoint,
        method: 'POST',
        form: requestParams
    };

    console.log('Authenticating with server...');
    return performRequest<any>(options).then<AccessToken>((body: { expires_on: any; access_token: any; }) =>
    {
        console.log("Got Token")
        var tok: AccessToken = {
            resource: resource,
            credentials: credentials,
            expiration: body.expires_on,
            token: body.access_token
        };

        return tok;
    }).catch(err => {
        console.log(err) ;
        throw err;
    });
}

/**
 * Transforms a promise so that it is tried again a specific number of times if it fails.
 *
 * A 'generator' of promises must be supplied. The reason is that if a promise fails,
 * then it will stay in a failed state and it won't be possible to await on it anymore.
 * Therefore a new promise must be returned every time.
 *
 * @param numRetries How many times should the promise be tried to be fulfilled.
 * @param promiseGenerator A function that will generate the promise to try to fulfill.
 * @param errPredicate In case an error occurs, receives the reason and returns whether to continue retrying
 */
export function withRetry<T>(
    numRetries: number,
    promiseGenerator: () => Q.Promise<T>,
    errPredicate?: ((err: any) => boolean)): Q.Promise<T>
{
    return promiseGenerator().fail((err: any) =>
    {
        if (numRetries > 0 && (!errPredicate || errPredicate(err)))
        {
            var randomDelay: number = Math.floor(Math.random() * RETRY_DELAY + RETRY_DELAY); // RETRY_DELAY <= randomDelay  < 2 * RETRY_DELAY
            console.log(`Operation failed with ${err}`);
            console.log(`Waiting ${randomDelay / 1000} seconds then retrying... (${numRetries - 1} retrie(s) left)`);
            return Q.delay(randomDelay).then(() => withRetry(numRetries - 1, promiseGenerator, errPredicate))
            .catch(err => {
                console.log(err) ;
                throw err;
            });
        }
        else
        {
            /* Don't wrap err in an error because it's already an error
            (.fail() is the equivalent of "catch" for promises) */
            throw err;
        }
    });
}

/**
 * Indicates whether the given object is an HTTP response for a retryable error.
 * @param err The error returned by the API
 * @param relax Whether the function will return true for most error codes or not
 * @description The Windows Store returns 429 and 503 for retryable errors. Relaxing the check will return true also for any error code greater or equal to 500
 */
export function isRetryableError(err:any, relax:boolean = true): boolean
{
    // Does this look like a ResponseInformation?
    if (err != undefined && err.response != undefined && typeof err.response.statusCode == 'number')
    {
        return err.response.statusCode == 429 // 429 code is returned by the API for throttle down. This is retriable
            || err.response.statusCode == 503
            || (relax && err.response.statusCode >= 500)
    }

    // Default to retry if no err information.
    return true;
}

/**
 * Examines a response body and logs errors and warnings.
 * @param response Response returned by the Store API
 * @param body A body in the format given by the Store API
 * (Where body.statusDetails.errors and body.statusDetails.warnings
 * are arrays of objects containing 'code' and 'details' attributes).
 */
function logErrorsAndWarnings(response: any, body: any)
{
    if (body === undefined || body.statusDetails === undefined)
        return;

    if (Array.isArray(body.statusDetails.errors) && body.statusDetails.errors.length > 0)
    {
        console.error('Errors occurred in request');
        (<any[]>body.statusDetails.errors).forEach(x => console.error(`\t[${x.code}]  ${x.details}`));
    }

    if (Array.isArray(body.statusDetails.warnings) && body.statusDetails.warnings.length > 0)
    {
        console.debug('Warnings occurred in request');
        (<any[]>body.statusDetails.warnings).forEach(x => console.debug(`\t[${x.code}]  ${x.details}`));
    }

    if (response != undefined &&
        response.headers['ms-correlationid'] != undefined)
    {
        console.debug(`CorrelationId: ${response.headers['ms-correlationid']}`);
    }
}