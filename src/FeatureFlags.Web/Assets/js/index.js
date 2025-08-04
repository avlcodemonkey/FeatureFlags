// import error handling first to try to log as many errors as possible
import './utils/onError';

// import custom elements next so they get defined as soon as possible.
import './components/PJax';
import './components/InfoDialog';
import './components/ConfirmDialog';
import './components/Alert';
import './components/Table';
import './components/DateFormatter';
import './components/JsonFormatter';
import './components/Autocomplete';
import './components/CopyToClipboard';
import './components/List';

// import anything else
import setupLuxbarToggle from './luxbar';

setupLuxbarToggle();
