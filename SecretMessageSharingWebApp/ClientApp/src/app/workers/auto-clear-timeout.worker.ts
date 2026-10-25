/// <reference lib="webworker" />

interface AutoClearTimeoutConfig {
	timeOutMs: number;
}

let autoClearTimeout: ReturnType<typeof setTimeout> | undefined;

// The owning page decides how long its content stays visible, because the recipient
// page and the sender page use different windows.
addEventListener('message', ({ data }: MessageEvent<AutoClearTimeoutConfig>) => {
	clearTimeout(autoClearTimeout);

	autoClearTimeout = setTimeout(() => {
		postMessage({ clear: true });
	}, data.timeOutMs);
});
