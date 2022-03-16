const path = require('path');
const fs = require('fs');
const marked = require('marked');
const sass = require('sass');
const hljs = require('highlight.js');

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
		href = '/' + href.replace(/\.md$/, '');

		return `<a href="${href.replace()}" title="${title ?? ''}">${text}</a>`;
	};

	hljs.default.registerLanguage('meml', (hljs) => {
		const ATTRIBUTE = {
			className: 'attr',
			begin: /(\\.|[^\\"\r\n])*(?=\s*:)/,
			relevance: 1.01
		};
		const PUNCTUATION = {
			match: /[{}[\]:]/,
			className: "punctuation",
			relevance: 0
		};
		const LITERALS = {
			beginKeywords: [
				"true",
				"false",
				"null",
			].join(" ")
		};

		const STRING = {
			scope: 'string',
			begin: "'",
			end: "'",
			illegal: '\\n',
			contains: [hljs.BACKSLASH_ESCAPE]
		}

		const NUMBER = {
			scope: 'number',
			begin: "(-?)(\\b0[xX][a-fA-F0-9]+|(\\b\\d+(\\.\\d*)?|\\.\\d+)([eE][-+]?\\d+)?)"
		}

		return {
			name: 'MEML',
			contains: [
				ATTRIBUTE,
				PUNCTUATION,
				hljs.QUOTE_STRING_MODE,
				LITERALS,
				hljs.HASH_COMMENT_MODE,
				STRING,
				NUMBER,
				{
					scope: 'number',
					begin: '\\*',
					end: '\\*',
					illegal: '\\n'
				}
			],
			illegal: '\\S'
		};
	});

	markdownRenderer.code = (code, language, isEscaped) => {
		return '<pre><code>' +
			hljs.default.highlight(language, code).value +
			'</code></pre>';
	};

	marked.marked.setOptions({ renderer: markdownRenderer, headerIds: true, gfm: true });

	const baseHtmlPage = fs.readFileSync('src/page.html').toString();

	// Delete old dist output
	console.log('Cleaning build results');
	fs.rmSync('dist', { recursive: true, force: true });
	fs.mkdirSync('dist');

	// Copy assets
	console.log('Copying assets');
	copyFolderRecursiveSync('src/assets', 'dist');
	// fs.cpSync('src/assets', 'dist/assets');

	// Compile styles
	console.log('Building styles');
	var css = sass.compile('src/styles.scss', { style: 'compressed' }).css;
	fs.writeFileSync('dist/styles.css', css);

	// Compile all markdown files to html
	findFilesRecursive('src', /\.md$/, (filename) => {
		console.log('Building', filename);

		let markdown = fs.readFileSync(filename).toString();
		let parsedHtml = marked.marked(markdown);

		let page = baseHtmlPage;

		page = page.replace(/\$TITLE\$/g, pageTitle);
		page = page.replace(/\$PAGE_CONTENT\$/, parsedHtml);

		let pagePath = '';
		if (filename === 'src/index.md') {
			pagePath = 'dist';
		}
		else {
			pagePath = filename.replace('src', 'dist').replace('.md', '');
			fs.mkdirSync(pagePath, { recursive: true });
		}

		fs.writeFileSync(pagePath + '/index.html', page);
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

function copyFileSync(source, target) {
	var targetFile = target;

	// If target is a directory, a new file with the same name will be created
	if (fs.existsSync(target)) {
		if (fs.lstatSync(target).isDirectory()) {
			targetFile = path.join(target, path.basename(source));
		}
	}

	fs.writeFileSync(targetFile, fs.readFileSync(source));
}

function copyFolderRecursiveSync(source, target) {
	var files = [];

	// Check if folder needs to be created or integrated
	var targetFolder = path.join(target, path.basename(source));
	if (!fs.existsSync(targetFolder)) {
		fs.mkdirSync(targetFolder);
	}

	// Copy
	if (fs.lstatSync(source).isDirectory()) {
		files = fs.readdirSync(source);
		files.forEach(function (file) {
			var curSource = path.join(source, file);
			if (fs.lstatSync(curSource).isDirectory()) {
				copyFolderRecursiveSync(curSource, targetFolder);
			} else {
				copyFileSync(curSource, targetFolder);
			}
		});
	}
}
