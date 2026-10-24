declare global {
	export interface String {
		linkify(): string;
	}
}

const escapeHtml = (value: string): string => value
	.replace(/&/g, '&amp;')
	.replace(/</g, '&lt;')
	.replace(/>/g, '&gt;')
	.replace(/"/g, '&quot;')
	.replace(/'/g, '&#39;');

const createLink = (href: string, text: string, openInNewTab: boolean): string => {
	const target = openInNewTab ? ' target="_blank" rel="noopener noreferrer"' : '';

	// Do not leak the retrieval URL (which contains the message id) to external sites.
	return `<a href="${escapeHtml(href)}"${target} referrerpolicy="no-referrer">${escapeHtml(text)}</a>`;
};

String.prototype.linkify = function () {

	// http://, https://, ftp://
	var urlPattern = /\b(?:https?|ftp):\/\/[a-z0-9-+&@#\/%?=~_|!:,.;]*[a-z0-9-+&@#\/%=~_|]/gim;

	// www. sans http:// or https://
	var pseudoUrlPattern = /(^|[^\/])(www\.[\S]+(\b|$))/gim;

	// Email addresses
	var emailAddressPattern = /[\w.]+@[a-zA-Z_-]+?(?:\.[a-zA-Z]{2,6})+/gim;

	// Links are replaced with placeholders before the remaining text is HTML-escaped.
	// This prevents message content from being interpreted as HTML (XSS) while still
	// producing safe anchors for URLs and email addresses.
	const links: string[] = [];
	const input = this.replace(/\u0000/g, '');

	const placeholder = (html: string): string => {
		const token = `\u0000LINK${links.length}\u0000`;
		links.push(html);

		return token;
	};

	let result = input
		.replace(urlPattern, match => placeholder(createLink(match, match, true)))
		.replace(pseudoUrlPattern, (_match, prefix: string, url: string) =>
			`${prefix}${placeholder(createLink(`https://${url}`, url, true))}`)
		.replace(emailAddressPattern, match => placeholder(createLink(`mailto:${match}`, match, false)));

	result = escapeHtml(result);

	links.forEach((link, index) => {
		// A replacer function is used so that '$' sequences inside the generated HTML
		// are not interpreted as replacement patterns.
		result = result.replace(`\u0000LINK${index}\u0000`, () => link);
	});

	return result;
};

export default String;
