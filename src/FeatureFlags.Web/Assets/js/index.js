// import custom elements first so they get defined as soon as possible.
import './components/PJax';
import './components/InfoDialog';
import './components/ConfirmDialog';
import './components/Alert';
import './components/Table';
import './components/DateFormatter';
import './components/JsonFormatter';
import './components/Autocomplete';

// import anything else
import setupLuxbarToggle from './luxbar';

setupLuxbarToggle();
