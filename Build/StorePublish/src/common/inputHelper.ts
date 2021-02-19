/*
 * General helper to process input variables.
 */


import path = require('path');

var glob = require('glob');

/**
* Get appropriate files from the provided pattern
* @param {string} path The minimatch pattern of glob to be resolved to file paths
* @returns {string[]} file paths resolved by glob
*/
export function resolvePathPattern(pathPattern: string) : string[]
{
    var filesList: string[] = [];
    if (pathPattern) {
        // Remove unnecessary quotes in path pattern, if any.
        pathPattern = pathPattern.replace(/\"/g, "");

        filesList = filesList.concat(glob.sync(pathPattern));
    }

    return filesList;
}

/**
 * Creates a canonical version of a path. Separators are converted to the current platform,
 * '.'.and '..' segments are resolved, and multiple contiguous separators are combined in one.
 * If a path contains both kinds of separators, it will be parsed as a posix path (with '/' separators).
 *
 * For example, the paths 'foo//bar/../quux.txt' and 'foo\\.\\quux.txt' should have the same canonical
 * representation.
 *
 * This function should be idempotent: canonicalizePath(canonicalizePath(x)) === canonicalizePath(x))
 * @param aPath
 */
export function canonicalizePath(aPath: string): string
{
    var pathObj: path.ParsedPath;
    if (aPath.indexOf('/') != -1)
    {
        pathObj = path.posix.parse(aPath);
    }
    else
    {
        pathObj = path.win32.parse(aPath);
    }

    return path.normalize(path.format(pathObj));
}