const path = require('path');
const fs = require('fs');
const sass = require('sass');
const cheerio = require('cheerio');
const minify = require('minify');

let pageTitle = '';

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

	const $ = cheerio.load(baseHtmlPage);

	$('title').text(pageTitle);
	$("#title").text(pageTitle);
	$("#main-content").html(parsedHtml);

	// page = page.replace(/\$TITLE\$/g, pageTitle);
	// page = page.replace(/\$PAGE_CONTENT\$/, parsedHtml);

	let pagePath = '';
	if (filename.replace('\\', '/') === 'src/index.md') {
		pagePath = 'dist';
	}
	else {
		pagePath = filename.replace('src', 'dist').replace('.md', '');
		fs.mkdirSync(pagePath, { recursive: true });
	}

	fs.writeFileSync(pagePath + '/index.html', $.html());
});
