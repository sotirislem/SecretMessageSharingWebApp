const PROXY_CONFIG = [
	{
		context: [
			"/api",
			"/swagger",
			"/signalR"
		],
		target: "https://localhost:7275",
		secure: false,
		logLevel: 'debug',
		ws: true
	}
]

module.exports = PROXY_CONFIG;
