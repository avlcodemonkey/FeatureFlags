// setup error handling first to try to log as many errors as possible
import onError from './utils/onError.js';
window.onerror = onError;

// import custom elements next so they get defined as soon as possible.
import './components/PJax.js';
import './components/InfoDialog.js';
import './components/ConfirmDialog.js';
import './components/Alert.js';
import './components/Table.js';
import './components/DateFormatter.js';
import './components/JsonFormatter.js';
import './components/Autocomplete.js';
import './components/CopyToClipboard.js';
import './components/List.js';
import './components/DisplayToggle.js';
import './components/SliderInput.js';
import './components/DateTimeInput.js';

// import anything else
import setupLuxbarToggle from './luxbar.js';

setupLuxbarToggle();
