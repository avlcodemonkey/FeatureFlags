# On Assets

The plan for assets is to focus on using native browser functionality and minimizing how much the build process does.  It's not possible to get everything working correctly with no build, so it uses `esbuild` to bundle & minify quickly.  Browser support is limited to modern browsers - no IE.

## Styles

All styling for the project uses native browser functionality with no LESS/SASS.  Modern browsers support nested CSS, so it's pretty easy to avoid LESS/SASS now.

Custom versions of `chota` and `luxbar` are added as bundled css.  These versions have a feature set trimmed down to just what is needed here, and use browser native functionality instead of SASS.  They should change infrequently.

It is possible to use the CSS without a build step if needed.  But the build will create sourcemaps that make debugging with the bundled/minified version easy still.

## Javascript

Javascript is used instead of typescript for this project.  Using javascript removes one layer of compilation and makes the code easier to write and debug.  JSDoc comments are used extensively, along with `@ts-check` to provide much of the functionality of typescript in javascript though.

The custom javascript is written as ES6 modules.  Because the project also contains external javascript using UMD format, a build step is required for the javascript.

## Fav Icon

Favicon.ico exists in both `Assets` and `wwwroot`.  It should always be the same file in both places.  Could create a build step to copy it from `Assets` to `wwwroot` but that seems like overkill.
