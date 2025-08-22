/**
 * Unit tests for onError function.
 */

import { describe, it, beforeEach, afterEach } from 'node:test';
import assert from 'node:assert';
import onError from '../../js/utils/onError.js';

describe('onError', () => {
    let originalSendBeacon;
    let originalConsoleError;
    let sendBeaconCalled;
    let sendBeaconArgs;
    let consoleErrorCalled;
    let consoleErrorArgs;

    beforeEach(() => {
        // Safely mock navigator.sendBeacon
        if (!global.navigator) {
            global.navigator = {};
        }
        originalSendBeacon = global.navigator.sendBeacon;
        sendBeaconCalled = false;
        sendBeaconArgs = null;
        global.navigator.sendBeacon = (url, body) => {
            sendBeaconCalled = true;
            sendBeaconArgs = { url, body };
            return true;
        };

        // Mock document.location.href
        if (!global.document) {
            global.document = {};
        }
        global.document.location = { href: 'http://localhost/test' };

        // Mock console.error
        originalConsoleError = global.console.error;
        consoleErrorCalled = false;
        consoleErrorArgs = null;
        global.console.error = (...args) => {
            consoleErrorCalled = true;
            consoleErrorArgs = args;
        };
    });

    afterEach(() => {
        // Restore mocks
        global.navigator.sendBeacon = originalSendBeacon;
        global.console.error = originalConsoleError;
    });

    it('does nothing if msg is falsy', async () => {
        await onError('', 'url', 1, 1, new Error('fail'));
        assert.strictEqual(sendBeaconCalled, false);
        assert.strictEqual(consoleErrorCalled, false);
    });

    it('calls sendBeacon with correct arguments', async () => {
        const error = new Error('fail');
        await onError('Test error', 'url', 1, 1, error);
        assert.strictEqual(sendBeaconCalled, true);
        assert.strictEqual(sendBeaconArgs.url, '/Error/LogJavascriptError');
        assert.ok(sendBeaconArgs.body instanceof FormData);
    });

    it('calls console.error if sendBeacon returns false', async () => {
        global.navigator.sendBeacon = () => false;
        await onError('Test error', 'url', 1, 1, new Error('fail'));
        assert.strictEqual(consoleErrorCalled, true);
        assert.ok(consoleErrorArgs[0].includes('Unable to log error to server'));
    });

    it('includes stack in FormData if error is provided', async () => {
        let stackValue = null;
        global.navigator.sendBeacon = (url, body) => {
            stackValue = body.get('stack');
            return true;
        };
        const error = new Error('fail');
        await onError('Test error', 'url', 1, 1, error);
        assert.strictEqual(stackValue, error.stack);
    });

    it('sets url in FormData to document.location.href', async () => {
        let urlValue = null;
        global.navigator.sendBeacon = (url, body) => {
            urlValue = body.get('url');
            return true;
        };
        await onError('Test error', 'url', 1, 1, new Error('fail'));
        assert.strictEqual(urlValue, 'http://localhost/test');
    });
});
