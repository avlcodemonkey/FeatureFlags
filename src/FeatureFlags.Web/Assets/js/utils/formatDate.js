/**
 * Formats a date. Subset of functionality from
 * fecha ~~ https://github.com/taylorhakes/fecha ~~ MIT License
 */

/**
 * @typedef {object} I18nSettings
 * @property {[string, string]} amPm - AM/PM strings
 * @property {string[]} dayNames - Full day names
 * @property {string[]} dayNamesShort - Short day names
 * @property {string[]} monthNames - Full month names
 * @property {string[]} monthNamesShort - Short month names
 * @property {(dayOfMonth: number) => string} DoFn - Ordinal suffix function
 */

/**
 * Regex for matching date format tokens.
 * @type {RegExp}
 */
const token = /d{1,4}|M{1,4}|YY(?:YY)?|S{1,3}|Do|ZZ|Z|([HhMsDm])\1?|[aA]|"[^"]*"|'[^']*'/g;

/**
 * Regex for matching literal text in format strings.
 * @type {RegExp}
 */
const literal = /\[([^]*?)\]/gm;

/**
 * Full day names.
 * @type {string[]}
 */
const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

/**
 * Full month names.
 * @type {string[]}
 */
const monthNames = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];

/**
 * Short day names.
 * @type {string[]}
 */
const dayNamesShort = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

/**
 * Short month names.
 * @type {string[]}
 */
const monthNamesShort = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

/**
 * Default internationalization settings.
 * @type {I18nSettings}
 */
const defaultI18n = {
    dayNamesShort: dayNamesShort,
    dayNames: dayNames,
    monthNamesShort: monthNamesShort,
    monthNames: monthNames,
    amPm: ['am', 'pm'],
    /**
     * Returns day of month with ordinal suffix (e.g., 1st, 2nd, 3rd, 4th).
     * @param {number} dayOfMonth - Day of the month (1-31).
     * @returns {string} Formatted day with suffix.
     */
    DoFn: function(dayOfMonth) {
        let suffixIndex;
        const lastDigit = dayOfMonth % 10;
        const isTeen = dayOfMonth - lastDigit === 10;
        if (lastDigit > 3) {
            suffixIndex = 0;
        } else {
            suffixIndex = (isTeen ? 0 : lastDigit);
        }
        return dayOfMonth + ['th', 'st', 'nd', 'rd'][suffixIndex];
    },
};

/**
 * Pads a value with leading zeros.
 * @param {string|number} val - Value to pad.
 * @param {number} len - Desired length.
 * @returns {string} Padded value as a string.
 */
const pad = function(val, len) {
    if (len === void 0) {
        len = 2;
    }
    val = String(val);
    while (val.length < len) {
        val = '0' + val;
    }
    return val;
};

/**
 * Formatting flags for date tokens.
 * Each property is a function that returns a formatted string for a given date.
 * @type {{ [key: string]: function(Date, I18nSettings): string }}
 */
const formatFlags = {
    D: function(dateObj) {
        return String(dateObj.getDate());
    },
    DD: function(dateObj) {
        return pad(dateObj.getDate());
    },
    Do: function(dateObj, i18n) {
        return i18n.DoFn(dateObj.getDate());
    },
    d: function(dateObj) {
        return String(dateObj.getDay());
    },
    dd: function(dateObj) {
        return pad(dateObj.getDay());
    },
    ddd: function(dateObj, i18n) {
        return i18n.dayNamesShort[dateObj.getDay()];
    },
    dddd: function(dateObj, i18n) {
        return i18n.dayNames[dateObj.getDay()];
    },
    M: function(dateObj) {
        return String(dateObj.getMonth() + 1);
    },
    MM: function(dateObj) {
        return pad(dateObj.getMonth() + 1);
    },
    MMM: function(dateObj, i18n) {
        return i18n.monthNamesShort[dateObj.getMonth()];
    },
    MMMM: function(dateObj, i18n) {
        return i18n.monthNames[dateObj.getMonth()];
    },
    YY: function(dateObj) {
        return pad(String(dateObj.getFullYear()), 4).slice(2);
    },
    YYYY: function(dateObj) {
        return pad(dateObj.getFullYear(), 4);
    },
    h: function(dateObj) {
        return String(dateObj.getHours() % 12 || 12);
    },
    hh: function(dateObj) {
        return pad(dateObj.getHours() % 12 || 12);
    },
    H: function(dateObj) {
        return String(dateObj.getHours());
    },
    HH: function(dateObj) {
        return pad(dateObj.getHours());
    },
    m: function(dateObj) {
        return String(dateObj.getMinutes());
    },
    mm: function(dateObj) {
        return pad(dateObj.getMinutes());
    },
    s: function(dateObj) {
        return String(dateObj.getSeconds());
    },
    ss: function(dateObj) {
        return pad(dateObj.getSeconds());
    },
    S: function(dateObj) {
        return String(Math.round(dateObj.getMilliseconds() / 100));
    },
    SS: function(dateObj) {
        return pad(Math.round(dateObj.getMilliseconds() / 10), 2);
    },
    SSS: function(dateObj) {
        return pad(dateObj.getMilliseconds(), 3);
    },
    a: function(dateObj, i18n) {
        return dateObj.getHours() < 12 ? i18n.amPm[0] : i18n.amPm[1];
    },
    A: function(dateObj, i18n) {
        return dateObj.getHours() < 12
            ? i18n.amPm[0].toUpperCase()
            : i18n.amPm[1].toUpperCase();
    },
    ZZ: function(dateObj) {
        const offset = dateObj.getTimezoneOffset();
        return ((offset > 0 ? '-' : '+')
          + pad(Math.floor(Math.abs(offset) / 60) * 100 + (Math.abs(offset) % 60), 4));
    },
    Z: function(dateObj) {
        const offset = dateObj.getTimezoneOffset();
        return ((offset > 0 ? '-' : '+')
          + pad(Math.floor(Math.abs(offset) / 60), 2)
          + ':'
          + pad(Math.abs(offset) % 60, 2));
    },
};

/**
 * Common date and time format masks.
 * @type {{ [key: string]: string }}
 */
const globalMasks = {
    default: 'ddd MMM DD YYYY HH:mm:ss',
    shortDate: 'M/D/YY',
    mediumDate: 'MMM D, YYYY',
    longDate: 'MMMM D, YYYY',
    fullDate: 'dddd, MMMM D, YYYY',
    isoDate: 'YYYY-MM-DD',
    isoDateTime: 'YYYY-MM-DDTHH:mm:ssZ',
    shortTime: 'HH:mm',
    mediumTime: 'HH:mm:ss',
    longTime: 'HH:mm:ss.SSS',
};

/***
 * Format a date.
 * @param {Date|number} dateObj - Date object or timestamp to format
 * @param {string} mask - Format of the date, i.e. 'mm-dd-yy' or 'shortDate'
 * @returns {string} Formatted date string
 */
const formatDate = function(dateObj, mask) {
    if (mask === void 0) {
        mask = globalMasks['default'];
    }
    if (typeof dateObj === 'number') {
        dateObj = new Date(dateObj);
    }
    if (Object.prototype.toString.call(dateObj) !== '[object Date]' || isNaN(dateObj.getTime())) {
        throw new Error('Invalid Date pass to format');
    }
    mask = globalMasks[mask] || mask;

    const literals = [];
    // Make literals inactive by replacing them with @@@
    mask = mask.replace(literal, function($0, $1) {
        literals.push($1);
        return '@@@';
    });

    // Apply formatting rules
    mask = mask.replace(token, function($0) {
        return formatFlags[$0](dateObj, defaultI18n);
    });

    // Inline literal values back into the formatted value
    return mask.replace(/@@@/g, function() {
        return literals.shift();
    });
};

export { formatDate };
