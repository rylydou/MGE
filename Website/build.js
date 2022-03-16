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

		let name = text.toLocaleLowerCase().replace(/[^a-z0-9 _-]/gi, '-').toLowerCase();

		return `<h${level} id="${name}">${text}</h${level}>`;
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
		const comment = {
			scope: 'comment',
			begin: '#',
			end: '[\n\r$]',
			relevance: 100
		}

		const type = {
			scope: 'type',
			match: /![\w\.:]+/g,
			relevance: 1,
		}

		const variable = {
			scope: 'variable',
			match: /\$\w+/g,
			relevance: 1,
		}

		const property = {
			scope: 'tag',
			begin: /\w+(?=:)/g,
		};

		const punctuation = {
			scope: "punctuation",
			match: /[{}[\]:]/,
		};

		const keywords = {
			scope: 'keyword',
			beginKeywords: 'true false null',
		};

		const string = {
			scope: 'string',
			begin: "'",
			beginScope: 'quote',
			end: "'",
			endScope: 'quote',
			illegal: '\\n',
			contains: [hljs.BACKSLASH_ESCAPE]
		}

		const number = {
			scope: 'number',
			match: /\d+(\.\d+)?.?/g,
		}

		const number_hex = {
			scope: 'number',
			match: /0[xX][0-9|a-f|A-F]+/g
		}

		const number_constants = {
			scope: 'number',
			match: '(nan)|[+-]?(inf)',
		};

		const binary = {
			scope: 'number',
			begin: '\\*',
			beginScope: 'quote',
			end: '\\*',
			endScope: 'quote',
			illegal: '\\n'
		};

		return {
			name: 'MEML',
			contains: [
				comment,
				punctuation,
				type,
				variable,
				property,
				keywords,
				string,
				binary,
				number,
				number_hex,
				number_constants,
			],
			illegal: '\\S'
		};
	});

	markdownRenderer.code = (code, language, isEscaped) => {
		return '<pre><code>' +
			hljs.default.highlight(code, { language: language }).value +
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
