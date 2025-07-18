/**
 * Unit tests for template rendering function.
 */

import {
    describe, expect, it,
} from 'vitest';
import renderTemplate from '../../js/utils/renderTemplate';

describe('renderTemplate', () => {
    it('replaces simple placeholders', () => {
        const template = 'Hello, {{name}}!';
        const data = { name: 'Alice' };
        expect(renderTemplate(template, data)).toBe('Hello, Alice!');
    });

    it('escapes HTML by default', () => {
        const template = 'Value: {{value}}';
        const data = { value: '<script>alert("x")</script>' };
        expect(renderTemplate(template, data)).toBe('Value: &lt;script&gt;alert(&quot;x&quot;)&lt;&#x2F;script&gt;');
    });

    it('does not escape HTML if escape is false', () => {
        const template = 'Value: {{value}}';
        const data = { value: '<b>bold</b>' };
        expect(renderTemplate(template, data, false)).toBe('Value: <b>bold</b>');
    });

    it('does not support nested properties', () => {
        const template = 'City: {{address.city}}';
        const data = { address: { city: 'Paris' } };
        expect(renderTemplate(template, data)).toBe('City: {{address.city}}');
    });

    it('renders placeholder string for missing properties', () => {
        const template = 'Hello, {{missing}}!';
        const data = {};
        expect(renderTemplate(template, data)).toBe('Hello, {{missing}}!');
    });

    it('handles conditional blocks (truthy)', () => {
        const template = 'Start {{#if isAdmin}}Admin{{/if}} End';
        const data = { isAdmin: true };
        expect(renderTemplate(template, data)).toBe('Start Admin End');
    });

    it('handles conditional blocks (falsy)', () => {
        const template = 'Start {{#if isAdmin}}Admin{{/if}} End';
        const data = { isAdmin: false };
        expect(renderTemplate(template, data)).toBe('Start  End');
    });

    it('handles notted conditional blocks', () => {
        const template = 'Start {{#if !isAdmin}}User{{/if}} End';
        const data = { isAdmin: false };
        expect(renderTemplate(template, data)).toBe('Start User End');
    });

    it('removes block if condition is falsy', () => {
        const template = '{{#if isAdmin}}Welcome{{/if}}';
        const data = { isAdmin: false };
        expect(renderTemplate(template, data)).toBe('');
    });

    it('ignores malformed if', () => {
        const template = '{{#if isAdmin)}}Welcome{{/if}}';
        const data = { isAdmin: false };
        expect(renderTemplate(template, data)).toBe('{{#if isAdmin)}}Welcome{{/if}}');
    });

    it('handles empty template', () => {
        expect(renderTemplate('', {})).toBe('');
    });
});
