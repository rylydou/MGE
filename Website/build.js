const fs = require('fs');
const sass = require('sass');
const cheerio = require('cheerio');
const builder = require('./builder.js');
const util = require('./util.js');
const path = require('path');

const baseHtmlPage = fs.readFileSync('src/page.html').toString();

// Delete old dist output
console.time('Cleaning build results');
fs.rmSync('dist', { recursive: true, force: true });
fs.mkdirSync('dist');
console.timeEnd('Cleaning build results');

// Copy assets
console.time('Copying assets');
util.copyFolderRecursiveSync('src/assets', 'dist');
console.timeEnd('Copying assets');
// fs.cpSync('src/assets', 'dist/assets');

// Compile styles
console.time('Building styles');
var css = sass.compile('src/styles.scss', { style: 'compressed' }).css;
fs.writeFileSync('dist/styles.css', css);
console.timeEnd('Building styles');

// Compile markdown pages
util.findFilesRecursive('src', /\.md$/, (filename) => {
	console.time(`Building ${filename}`);
	let pageMarkdown = fs.readFileSync(filename).toString();
	let builtPage = builder.buildPage(pageMarkdown);
	console.timeEnd(`Building ${filename}`);

	const $ = cheerio.load(baseHtmlPage);

	$('title').text(builtPage.title + " - MGE");
	$('#title').text(builtPage.title);
	$('#main-content').html(builtPage.html);

	let pagePath = filename;

	if (filename.endsWith('index.md')) {
		pagePath = path.dirname(pagePath);
	}

	pagePath = pagePath.replace('src', 'dist').replace(/\.md$/, '');

	if (!fs.existsSync(!pagePath)) fs.mkdirSync(pagePath, { recursive: true });

	fs.writeFileSync(pagePath + '/index.html', $.html());
});
