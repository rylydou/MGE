const marked = require('marked')
const hljs = require('highlight.js')
const util = require('./util.js')

var title = ''
var dir = ''

const markdownRenderer = new marked.Renderer()

markdownRenderer.heading = (text, level) => {
	if (level == 1) {
		if (title == '')
			title = text
		return ''
	}

	let id = text.toLocaleLowerCase().replace(/[^a-z0-9 _-]/gi, '-').toLowerCase()

	return `<h${level}><a href="#${id}" id="${id}">#</a> ${text}</h${level}>`
}

markdownRenderer.image = (href, title, text) => {
	if (!text) console.log(`[!] '${href}' is missing alt text`)
	return `<img src="${href}" alt="${text ?? 'Image'}" title="${title ?? ''}"/>`
}

markdownRenderer.link = (href, title, text) => {
	// If its a external link then treat it specially
	if (href.startsWith('http'))
		return `<a href="${href}" title="${title ?? ''}" target="_blank">${text}</a>`

	if (href.startsWith('./'))
		href = `${dir}/${util.convertPath(href)}`
	else
		href = util.convertPath(href)

	return `<a href="${href.replace()}" title="${title ?? ''}">${text}</a>`
}

markdownRenderer.code = (code, language, isEscaped) => {
	let html = hljs.default.highlight(code, { language: language }).value

	html = html.replace(/    /g, '\t')

	return '<pre><code>' +
		html +
		'</code></pre>'
}

// Code highlighting
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
	}

	const punctuation = {
		scope: "punctuation",
		match: /[{}[\]:]/,
	}

	const keywords = {
		scope: 'keyword',
		beginKeywords: 'true false null',
	}

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
		match: '(nan)|([+-]?inf)',
	}

	const binary = {
		scope: 'number',
		begin: '\\*',
		beginScope: 'quote',
		end: '\\*',
		endScope: 'quote',
		illegal: '\\n'
	}

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
	}
})

exports.buildPage = (markdown, dirname) => {
	title = ''
	dir = dirname
	let html = marked.marked(markdown, { renderer: markdownRenderer, gfm: true, smartLists: true, })

	return { html, title }
}
