const PROXY_CONFIG = [
	{
		context: [
			"/api",
			"/swagger"
		],
		target: "https://localhost:7275",
		secure: false
	}
]

module.exports = PROXY_CONFIG;
