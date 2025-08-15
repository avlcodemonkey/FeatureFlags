/**
 * Unit tests for template rendering function.
 */

import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import renderTemplate from '../../js/utils/renderTemplate.js';

describe('renderTemplate', () => {
    it('replaces simple placeholders', () => {
        const template = 'Hello, {{name}}!';
        const data = { name: 'Alice' };
        assert.strictEqual(renderTemplate(template, data), 'Hello, Alice!');
    });

    it('escapes HTML by default', () => {
        const template = 'Value: {{value}}';
        const data = { value: '<script>alert("x")</script>' };
        assert.strictEqual(
            renderTemplate(template, data),
            'Value: &lt;script&gt;alert(&quot;x&quot;)&lt;&#x2F;script&gt;',
        );
    });

    it('does not escape HTML if escape is false', () => {
        const template = 'Value: {{value}}';
        const data = { value: '<b>bold</b>' };
        assert.strictEqual(renderTemplate(template, data, false), 'Value: <b>bold</b>');
    });

    it('does not support nested properties', () => {
        const template = 'City: {{address.city}}';
        const data = { address: { city: 'Paris' } };
        assert.strictEqual(renderTemplate(template, data), 'City: {{address.city}}');
    });

    it('renders placeholder string for missing properties', () => {
        const template = 'Hello, {{missing}}!';
        const data = {};
        assert.strictEqual(renderTemplate(template, data), 'Hello, {{missing}}!');
    });

    it('handles conditional blocks (truthy)', () => {
        const template = 'Start {{#if isAdmin}}Admin{{/if}} End';
        const data = { isAdmin: true };
        assert.strictEqual(renderTemplate(template, data), 'Start Admin End');
    });

    it('handles conditional blocks (falsy)', () => {
        const template = 'Start {{#if isAdmin}}Admin{{/if}} End';
        const data = { isAdmin: false };
        assert.strictEqual(renderTemplate(template, data), 'Start  End');
    });

    it('handles notted conditional blocks', () => {
        const template = 'Start {{#if !isAdmin}}User{{/if}} End';
        const data = { isAdmin: false };
        assert.strictEqual(renderTemplate(template, data), 'Start User End');
    });

    it('removes block if condition is falsy', () => {
        const template = '{{#if isAdmin}}Welcome{{/if}}';
        const data = { isAdmin: false };
        assert.strictEqual(renderTemplate(template, data), '');
    });

    it('ignores malformed if', () => {
        const template = '{{#if isAdmin)}}Welcome{{/if}}';
        const data = { isAdmin: false };
        assert.strictEqual(renderTemplate(template, data), '{{#if isAdmin)}}Welcome{{/if}}');
    });

    it('handles empty template', () => {
        assert.strictEqual(renderTemplate('', {}), '');
    });

    it('returns empty string for null template', () => {
        assert.strictEqual(renderTemplate(null, { name: 'Alice' }), '');
    });

    it('returns empty string for undefined template', () => {
        assert.strictEqual(renderTemplate(undefined, { name: 'Alice' }), '');
    });

    it('returns empty string for template with only whitespace', () => {
        assert.strictEqual(renderTemplate('   ', { name: 'Alice' }), '   ');
    });

    it('returns template as-is for non-object data', () => {
        const template = 'Hello, {{name}}!';
        assert.strictEqual(renderTemplate(template, null), 'Hello, {{name}}!');
        assert.strictEqual(renderTemplate(template, undefined), 'Hello, {{name}}!');
        assert.strictEqual(renderTemplate(template, 42), 'Hello, {{name}}!');
        assert.strictEqual(renderTemplate(template, 'string'), 'Hello, {{name}}!');
    });

    it('handles multiple and adjacent placeholders', () => {
        const template = '{{greeting}}, {{name}}! {{greeting}}{{punct}}';
        const data = { greeting: 'Hi', name: 'Bob', punct: '!' };
        assert.strictEqual(renderTemplate(template, data), 'Hi, Bob! Hi!');
    });

    it('handles special characters in keys', () => {
        const template = 'Key: {{user-name_1}}';
        const data = { 'user-name_1': 'special' };
        assert.strictEqual(renderTemplate(template, data), 'Key: special');
    });

    it('handles already escaped HTML', () => {
        const template = 'Safe: {{value}}';
        const data = { value: '&lt;b&gt;bold&lt;/b&gt;' };
        assert.strictEqual(renderTemplate(template, data), 'Safe: &amp;lt;b&amp;gt;bold&amp;lt;&#x2F;b&amp;gt;');
    });

    it('does not throw for template with no placeholders', () => {
        const template = 'No placeholders here.';
        const data = { name: 'Alice' };
        assert.strictEqual(renderTemplate(template, data), 'No placeholders here.');
    });

    it('handles empty data object', () => {
        const template = 'Hello, {{name}}!';
        assert.strictEqual(renderTemplate(template, {}), 'Hello, {{name}}!');
    });

    it('handles undefined values in data', () => {
        const template = 'Hello, {{name}}!';
        const data = { name: undefined };
        assert.strictEqual(renderTemplate(template, data), 'Hello, !');
    });

    it('handles null values in data', () => {
        const template = 'Hello, {{name}}!';
        const data = { name: null };
        assert.strictEqual(renderTemplate(template, data), 'Hello, !');
    });
});
