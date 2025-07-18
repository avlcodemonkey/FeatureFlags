# Creating Icons

## Intro

This project uses inline SVG for icons instead of an icon font.  This is more accessible without sacrificing much performance-wise.  Mono Icons is the base icon set.  All the icons in that set have been optimized and converted to `cshtml` partials to use with Razor.

### mono-icons
https://github.com/mono-company/mono-icons

https://icons.mono.company/

## C# Enum

This snippet converts the icon JSON into a pascal case list that can be used to create a C# enum.

JSON is from `mono-icons/iconfont/icons.json` with filtering to remove `\\` and linebreaks.

    
    const icons = '{ "add": "f101", "archive": "f102", "arrow-down": "f103", "arrow-left-down": "f104", "arrow-left-up": "f105", "arrow-left": "f106", "arrow-right-down": "f107", "arrow-right-up": "f108", "arrow-right": "f109", "arrow-up": "f10a", "attachment": "f10b", "backspace": "f10c", "ban": "f10d", "bar-chart-alt": "f10e", "bar-chart": "f10f", "board": "f110", "bold": "f111", "book": "f112", "bookmark": "f113", "calendar": "f114", "call": "f115", "camera": "f116", "caret-down": "f117", "caret-left": "f118", "caret-right": "f119", "caret-up": "f11a", "check": "f11b", "chevron-double-down": "f11c", "chevron-double-left": "f11d", "chevron-double-right": "f11e", "chevron-double-up": "f11f", "chevron-down": "f120", "chevron-left": "f121", "chevron-right": "f122", "chevron-up": "f123", "circle-add": "f124", "circle-arrow-down": "f125", "circle-arrow-left": "f126", "circle-arrow-right": "f127", "circle-arrow-up": "f128", "circle-check": "f129", "circle-error": "f12a", "circle-help": "f12b", "circle-information": "f12c", "circle-remove": "f12d", "circle-warning": "f12e", "circle": "f12f", "clipboard-check": "f130", "clipboard-list": "f131", "clipboard": "f132", "clock": "f133", "close": "f134", "cloud-download": "f135", "cloud-upload": "f136", "cloud": "f137", "cloudy": "f138", "comment": "f139", "compass": "f13a", "computer": "f13b", "copy": "f13c", "credit-card": "f13d", "database": "f13e", "delete-alt": "f13f", "delete": "f140", "document-add": "f141", "document-check": "f142", "document-download": "f143", "document-empty": "f144", "document-remove": "f145", "document": "f146", "download": "f147", "drag": "f148", "drop": "f149", "edit-alt": "f14a", "edit": "f14b", "email": "f14c", "enter": "f14d", "expand": "f14e", "export": "f14f", "external-link": "f150", "eye-off": "f151", "eye": "f152", "favorite": "f153", "filter-1": "f154", "filter-alt": "f155", "filter": "f156", "flag": "f157", "fog": "f158", "folder-add": "f159", "folder-check": "f15a", "folder-download": "f15b", "folder-remove": "f15c", "folder": "f15d", "grid": "f15e", "heart": "f15f", "home": "f160", "image": "f161", "inbox": "f162", "italic": "f163", "laptop": "f164", "layers": "f165", "layout": "f166", "link-alt": "f167", "link": "f168", "list": "f169", "location": "f16a", "lock": "f16b", "log-in": "f16c", "log-out": "f16d", "map": "f16e", "megaphone": "f16f", "menu": "f170", "message-alt": "f171", "message": "f172", "minimize": "f173", "mobile": "f174", "moon": "f175", "next": "f176", "notification-off": "f177", "notification": "f178", "options-horizontal": "f179", "options-vertical": "f17a", "pause": "f17b", "pen": "f17c", "percentage": "f17d", "pin": "f17e", "play": "f17f", "previous": "f180", "print": "f181", "rain": "f182", "refresh": "f183", "remove": "f184", "reorder-alt": "f185", "reorder": "f186", "repeat": "f187", "save": "f188", "search": "f189", "select": "f18a", "send": "f18b", "settings": "f18c", "share": "f18d", "shopping-cart-add": "f18e", "shopping-cart": "f18f", "shuffle": "f190", "snow": "f191", "snowflake": "f192", "sort": "f193", "speakers": "f194", "stop": "f195", "storm": "f196", "strikethrough": "f197", "sun": "f198", "sunrise-1": "f199", "sunrise": "f19a", "sunset": "f19b", "switch": "f19c", "table": "f19d", "tablet": "f19e", "tag": "f19f", "temperature": "f1a0", "text": "f1a1", "three-rows": "f1a2", "two-columns": "f1a3", "two-rows": "f1a4", "underline": "f1a5", "undo": "f1a6", "unlock": "f1a7", "user-add": "f1a8", "user-check": "f1a9", "user-remove": "f1aa", "user": "f1ab", "users": "f1ac", "volume-off": "f1ad", "volume-up": "f1ae", "warning": "f1af", "webcam": "f1b0", "wind": "f1b1", "window": "f1b2", "zoom-in": "f1b3", "zoom-out": "f1b4" }';
    
    console.log(Array.from(Object.keys(JSON.parse(icons))).map((x) => x.replace(/(^\w|-\w)/g, (y) => y.replace(/-/, '').toUpperCase())));

## Razor Partials

Optimized SVG files are stored in `Views/Shared/Icons` as `cshtml` files that can be used as a partial within Razor views.  New icons can be added by 1) adding the name to the enum, and 2) adding a `cshtml` file in the `Views/Shared/Icons` folder.

SVGO is included as a dev dependency and can be used to optimize SVG files to use for the partials.  Run `npm run svgo` to optimize `svg` files in the `Assets/icons/svg` folder.  The rename icons script below was also created to bulk rename Mono Icon files.

Icon partials should include `aria-hidden="true" focusable="false"` in the `svg` element for better accessibility.  See https://www.sarasoueidan.com/blog/accessible-icon-buttons/ for more details.


## Rename Icons

Bulk rename svg files from Mono Icon, ke-bab case to Pascal case. IE `optimized/arrow-left.svg` => `optimized/_ArrowLeft.cshtml`.

    const fs = require('fs');
    const path = require('path');

    const folderPath = './optimized';

    /**
     * Capitalizes the first letter of a string.
     * @param {string} string String to capitalize.
     * @returns {string} Capitalized string.
     */
    function capitalizeFirstLetter(string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }

    /**
     * Convert string to pascal case from snake case.
     * @param {string} input String to convert.
     * @returns {string} Converted string.
     */
    function getPascalFromSnake(input) {
        return input.split('-').map(capitalizeFirstLetter).join('');
    }

    // read all files in the directory
    const filesArr = fs.readdirSync(folderPath);

    // Loop through array and rename all files
    filesArr.forEach((file) => {
        const fullPath = path.join(folderPath, file);
        const fileName = path.basename(`_${getPascalFromSnake(file)}`, 'svg');

        const newFileName = `${fileName}cshtml`;
        try {
            console.log(newFileName);
            fs.renameSync(fullPath, path.join(folderPath, newFileName));
        } catch (error) {
            console.error(error);
        }
    });
