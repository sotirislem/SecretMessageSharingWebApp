/// <reference lib="webworker" />

import { Constants } from "../../../constants";


const clearTimeout = Constants.AUTOCLEAR_INTERVAL_MINUTES * (60 * 1000);

setTimeout(() => {
	postMessage({ clear: true });
}, clearTimeout);
