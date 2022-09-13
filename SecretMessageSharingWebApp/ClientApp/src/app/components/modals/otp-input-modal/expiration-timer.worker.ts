/// <reference lib="webworker" />

import { Constants } from "../../../../constants";


let timer: any;
let timerRemainingSeconds = Constants.OTP_EXPIRATION_MINUTES * 60;
let timerExpirationTime = '';

const timerFunc = () => {
	--timerRemainingSeconds;

	const m = Math.floor(timerRemainingSeconds / 60);
	const s = timerRemainingSeconds - (m * 60);

	const minutes = m < 10 ? "0" + m : m;
	const seconds = s < 10 ? "0" + s : s;

	timerExpirationTime = minutes + ":" + seconds;

	if (timerRemainingSeconds == 0) {
		clearInterval(timer);
	}

	postMessage({ timerRemainingSeconds, timerExpirationTime });
};

timerFunc();
timer = setInterval(timerFunc, 1000);
