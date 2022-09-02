declare global {
	export interface String {
		linkify(): string;
	}
}

String.prototype.linkify = function () {

	// http://, https://, ftp://
	var urlPattern = /\b(?:https?|ftp):\/\/[a-z0-9-+&@#\/%?=~_|!:,.;]*[a-z0-9-+&@#\/%=~_|]/gim;

	// www. sans http:// or https://
	var pseudoUrlPattern = /(^|[^\/])(www\.[\S]+(\b|$))/gim;

	// Email addresses
	var emailAddressPattern = /[\w.]+@[a-zA-Z_-]+?(?:\.[a-zA-Z]{2,6})+/gim;

	return this
		.replace(urlPattern, '<a href="$&" target="_blank">$&</a>')
		.replace(pseudoUrlPattern, '$1<a href="https://$2" target="_blank">$2</a>')
		.replace(emailAddressPattern, '<a href="mailto:$&">$&</a>');
};

export default String;
