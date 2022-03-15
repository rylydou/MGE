// import { marked } from "marked";
// import { fs } from "node";
// import { path } from "path";
const path = require('path');
const fs = require('fs');
const marked = require('marked');
const sass = require('sass');

main();

function main() {
	const markdownRenderer = new marked.Renderer();

	let pageTitle = '';

	markdownRenderer.heading = (text, level) => {
		if (level === 1) {
			pageTitle = text;
			return '';
		}

		return `<h${level}>${text}</h${level}>`;
	};

	markdownRenderer.link = (href, title, text) => {
		// If its a external link then treat it specially
		if (href.startsWith('http')) {
			return `<a href="${href}" title="${title ?? ''}" target="_blank">${text}</a>`;
		}

		// Change references from markdown to html
		href = href.replace(/\.md$/, '.html');

		return `<a href="${href}" title="${title ?? ''}">${text}</a>`;
	};

	marked.marked.setOptions({ renderer: markdownRenderer, headerIds: true, gfm: true, });

	const baseHtmlPage = fs.readFileSync('src/page.html').toString();

	// Delete old build output
	fs.rmSync('build', { recursive: true, force: true });
	fs.mkdirSync('build');

	// Compile styles
	console.log('Building styles');
	var css = sass.compile('src/styles/styles.scss', { style: 'compressed' }).css;
	fs.writeFileSync('build/styles.css', css);

	// Compile all markdown files to html
	findFilesRecursive('src', /\.md$/, (filename) => {
		console.log('Building ', filename);

		let markdown = fs.readFileSync(filename).toString();
		let parsedHtml = marked.marked(markdown);

		let page = baseHtmlPage;

		page = page.replace(/\$TITLE\$/g, pageTitle);
		page = page.replace('$PAGE_CONTENT$', parsedHtml);

		// String.replace on replaces the first instance right?
		let htmlFilePath = filename.replace('src', 'build').replace(/\.md$/, '.html');
		let htmlFolderPath = path.dirname(htmlFilePath);

		if (!fs.existsSync(htmlFolderPath)) {
			fs.mkdirSync(htmlFolderPath, { recursive: true });
		}

		fs.writeFileSync(htmlFilePath, page);
	});
}

function findFilesRecursive(startPath, filter, callback) {
	let files = fs.readdirSync(startPath);
	for (var i = 0; i < files.length; i++) {
		let filename = path.join(startPath, files[i]);
		let stat = fs.lstatSync(filename);
		if (stat.isDirectory()) {
			findFilesRecursive(filename, filter, callback); // Recurse
		}
		else if (filter.test(filename)) callback(filename);
	};
};
