import globals from 'globals';
import js from '@eslint/js';
import stylistic from '@stylistic/eslint-plugin';
import jsdoc from 'eslint-plugin-jsdoc';

export default [
    {
        files: ['**/*.{js,mjs,cjs}'],
        ignores: ['**/node_modules/**/*', '**/wwwroot/**/*'],
        languageOptions: {
            globals: { ...globals.browser, ...globals.node },
        },
        plugins: {
            js,
            '@stylistic': stylistic,
            jsdoc,
        },
        rules: {
            // add standard rules
            ...js.configs.recommended.rules,
            // add stylistic rules
            ...stylistic.configs.recommended.rules,
            // jsdoc rules
            ...jsdoc.configs['flat/recommended'].rules,
            // custom rule overrides
            'linebreak-style': ['error', 'windows'],
            'indent': ['error', 4],
            '@stylistic/indent': ['error', 4],
            '@stylistic/lines-between-class-members': 'off',
            '@stylistic/space-before-function-paren': 'off',
            '@stylistic/curly-newline': ['error', 'always'],
            '@stylistic/max-len': ['error', { code: 160 }],
            '@stylistic/semi': 'error',
            '@stylistic/brace-style': 'error',
            'prefer-global-this': 'off',
        },
    },
];
