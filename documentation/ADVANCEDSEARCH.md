# Advanced Search

This form brings great search capabilities to TQVault.
It can not only help find items but also theory craft.

_**This tool is meant to be used with "Preload All" setting enabled.**_

_**The first load can be long but reopening the form si fast.**_

---

## Table of contents
+ [Fulltext search](#Fulltext)
+ [Regex search](#Regex)
+ [Category visibility](#ShowHide)
+ [Category filtering](#CategoryFilter)
+ [Uncheck all](#UncheckAll)
+ [Display match during selection](#DispMatch)
+ [Reduce categories during selection](#Reduce)
+ [Logical operator](#AndOr)
+ [Query persistance](#Query)
+ [Flexible visibility](#Flexible)
+ [Preview tooltip](#Preview)
+ [Category tooltip](#CategoryPreview)

---

## <a id="UI"></a>UI
Here's a brief overview of some UI features.

---

### <a id="Fulltext"></a>Fulltext search
By default, the input is a literal search.

![ShowHide](advancedsearch/fulltext.png)

---

### <a id="Regex"></a>Regex search

By using `/` as first char everything else is a standard [C# regular expression](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference).

Here's few examples.

- `/rune|earth` : Everything with "rune" or "earth"
- `/chance of .+%.+bleeding damage` : All chance of + percent bleeding damage
- `/\+.+%.+(life|mana) leech` : Any + percentage of life or mana leech
- `/\+.+%.+(cold|fire) damage` : Any + percentage of cold or fire leech

![ShowHide](advancedsearch/regex.png)

---

### <a id="ShowHide"></a>Category visibility
Some categories may have no interest to you.

You can hide it.

![ShowHide](advancedsearch/showhide.png)

---

### <a id="CategoryFilter"></a>Category filtering
looking for specific categories?

It accept [regular expressions](#Regex).

![Category filtering](advancedsearch/categoryfilter.png)

---

### <a id="UncheckAll"></a>Uncheck all
You can uncheck all selected attributes inside a category by 'right-clicking' the yellow category label.

---

### <a id="DispMatch"></a>Display match during selection
The item count adjust in real time.

![Display Match](advancedsearch/displaymatch.png)

---

### <a id="Reduce"></a>Reduce categories during selection
When that checkbox is enabled, each selection reduce further available categories and items.

![Display Match](advancedsearch/reduce.png)

---

### <a id="AndOr"></a>Logical operator
This let you choose logical mode for item filtering.
- _**And**_ : Items must comply **to all** selected categories (Default). 
- _**Or**_ :  Items must comply **to at least one** selected categories .

![Logical operator](advancedsearch/andor.png)

---

### <a id="Query"></a>Query persistance
You can save and reload a filter selection.
Don't loose time when you track something complex.

![Query persistance](advancedsearch/query.png)

---

### <a id="Flexible"></a>Flexible visibility
You can adjust the number of visible elements per category.

![Flexible visibility](advancedsearch/flexible.png)

---

### <a id="Preview"></a>Preview tooltip
Hovering on the item count in the bottom of the windows display a popup with a preview of found items.

![Tooltip preview](advancedsearch/tooltippreview.png)

---

### <a id="CategoryPreview"></a>Category preview
Hovering on a category display a tooltip with the number of related items.

![Tooltip preview](advancedsearch/tooltipitemcount.png)

