const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT
	? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
	:  env.ASPNETCORE_URLS
		? env.ASPNETCORE_URLS.split(';')[0]
		: 'https://localhost:7275';

console.log(`[Proxy] ASPNETCORE_HTTPS_PORT: ${env.ASPNETCORE_HTTPS_PORT}`);
console.log(`[Proxy] ASPNETCORE_URLS: ${env.ASPNETCORE_URLS}`);
console.log(`[Proxy] Calculated target: ${target}`);

const PROXY_CONFIG = [
	{
		context: [
			"/api",
			"/swagger",
			"/signalr"
		],
		target: target,
		secure: false,
		logLevel: 'debug',
		ws: true
	}
]

module.exports = PROXY_CONFIG;
