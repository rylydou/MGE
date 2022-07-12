const fs = require('fs')
const sass = require('sass')
const cheerio = require('cheerio')
const builder = require('./builder.js')
const util = require('./util.js')
const settings = require('./settings.js')
const path = require('path')

// Delete old dist output
console.time('Cleaning build results')
fs.rmSync('dist', { recursive: true, force: true })
fs.mkdirSync('dist')
console.timeEnd('Cleaning build results')

// Copy assets
console.time('Copying assets')
util.copyFolderRecursiveSync(`${settings.source}/assets`, 'dist')
console.timeEnd('Copying assets')
// fs.cpSync('src/assets', 'dist/assets')

// Compile styles
console.time('Building styles')
var css = sass.compile(`${settings.source}/styles.scss`, { style: 'compressed' }).css
fs.writeFileSync(`${settings.destination}/styles.css`, css)
console.timeEnd('Building styles')

const pages = []

// Compile markdown pages
util.findFilesRecursive('src', /\.md$/, (filename) => {
	console.time(`Building ${filename}`)

	let page = {}

	let url = filename
	// Remove `src/`, will get replaced with `dist/`
	url = url.replace(`${settings.source}/`, '')
	url = util.convertPath(url)
	page.url = url

	page.path = filename

	let markdown = fs.readFileSync(filename).toString()
	page = Object.assign(page, builder.buildPage(markdown, url))

	console.timeEnd(`Building ${filename}`)

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
			str += (key[0].toUpperCase() + key.substring(1).replace(/-/g, ' '))
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

const pageSkeleton = fs.readFileSync(`${settings.source}/page.html`)

pages.forEach(page => {
	let dest = `${settings.destination}/${page.url}`

	// Populate the html page
	const $ = cheerio.load(pageSkeleton)
	$('title').text(`${page.title} - MGE`)
	$('#title').text(page.title)
	$('#main-content').html(page.html)
	$('#toc').append(tocHTML)
	$('#edit').attr('href', `${settings.editURL}/${page.path}`)

	// Create the folders leading up to the file
	if (!fs.existsSync(dest)) fs.mkdirSync(dest, { recursive: true })
	// Write the html file
	fs.writeFileSync(dest + '/index.html', $.html())
})
