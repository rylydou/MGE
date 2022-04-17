const fs = require('fs')
const sass = require('sass')
const cheerio = require('cheerio')
const builder = require('./builder.js')
const util = require('./util.js')

// Delete old dist output
console.time('Cleaning build results')
fs.rmSync('dist', { recursive: true, force: true })
fs.mkdirSync('dist')
console.timeEnd('Cleaning build results')

// Copy assets
console.time('Copying assets')
util.copyFolderRecursiveSync('src/assets', 'dist')
console.timeEnd('Copying assets')
// fs.cpSync('src/assets', 'dist/assets')

// Compile styles
console.time('Building styles')
var css = sass.compile('src/styles.scss', { style: 'compressed' }).css
fs.writeFileSync('dist/styles.css', css)
console.timeEnd('Building styles')

const pages = []

// Compile markdown pages
util.findFilesRecursive('src', /\.md$/, (filename) => {
	console.time(`Building ${filename}`)

	let markdown = fs.readFileSync(filename).toString()
	let page = builder.buildPage(markdown)

	console.timeEnd(`Building ${filename}`)

	let url = filename
	// Remove the number and space at the beginning of the file and folders used for ordering
	url = filename.replace(/\d+\s/g, '')
	// Remove `src/`, will get replaced with `dist/`
	url = url.replace(/^src\//, '')
	// Remove `.md`, will get changed to `.html`
	url = url.replace(/\.md$/, '')
	// Remove `/index`, special case to handle index files
	url = url.replace(/\/?index$/, '')

	page.url = url

	pages.push(page)
})

let toc = {}

pages.forEach(page => {
	let segments = page.url.split('/')

	let current = toc
	for (const segment of segments) {
		if (current[segment] == undefined)
			current[segment] = {}
		current = current[segment]
	}
	current._title = page.title
	current._href = '/' + page.url
})

function populate(root) {
	let str = '<ul role="tree">'

	for (const key in root) {
		if (key == '') continue
		if (key.startsWith('_')) continue

		let item = root[key]

		str += '<li role="treeitem" open>'

		let hasHREF = item._href != undefined
		if (hasHREF) str += `<a href="${item._href}">`
		if (item._title == undefined)
			str += (key[0].toUpperCase() + key.substring(1).replace(/-/g, ''))
		else str += item._title
		if (hasHREF) str += `</a>`

		str += populate(item)

		str += '</li>'
	}

	str += '</ul>'
	return str
}

console.time('Generate TOC')
var tocHTML = populate(toc)
console.timeEnd('Generate TOC')

// console.log(JSON.stringify(toc, null, 2))

const pageSkeleton = fs.readFileSync('src/page.html')

pages.forEach(page => {
	let dest = 'dist/' + page.url

	// Populate the html page
	const $ = cheerio.load(pageSkeleton)
	$('title').text(page.title + " - MGE")
	$('#title').text(page.title)
	$('#main-content').html(page.html)
	$('#toc').append(tocHTML)

	// Create the folders leading up to the file
	if (!fs.existsSync(dest)) fs.mkdirSync(dest, { recursive: true })
	// Write the html file
	fs.writeFileSync(dest + '/index.html', $.html())
})
