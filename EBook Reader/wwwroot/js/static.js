/*
  Lab2.js - Helper functions for theming static pages

  Narendra Katamaneni, CSE686 - Internet Programming, Spring 2019
*/

//function call on page load to populate theme.
function loadItems() {
	generateDate();
	setTheme();
}

function loadPage() {
	setTheme();
}


function setBodyTheme(theme) {
	//var test = document.getElementsByClassName(theme)[0];
	// if (test) {
	var elem = document.getElementsByTagName('body')[0];
	if (elem.classList.contains(theme))
		return;
	elem.classList.add(theme);
	// }
}
/*---- set class attribute on all links -----------------*/

function setLinkTheme(theme) {
	for (let i = 0, l = document.links.length; i < l; ++i) {
		let lnk = document.links[i];
		if (lnk.classList.contains(theme))
			return;
		lnk.classList.add(theme);
	}
}
/*---- change class attribute on body -------------------*/

function swapBodyTheme(oldTheme, newTheme) {
	var test = document.getElementsByClassName(oldTheme)[0];
	if (test) {
		let elem = document.getElementsByTagName('body')[0];
		if (elem.classList.contains(oldTheme))
			elem.classList.remove(oldTheme);
		elem.classList.add(newTheme);
	}
}
/*---- change class attributes on all links -------------*/

function swapLinkTheme(oldTheme, newTheme) {
	let test = document.getElementsByClassName(oldTheme)[0];
	if (!test) {
		return;
	}
	for (let i = 0, l = document.links.length; i < l; ++i) {
		let lnk = document.links[i];
		if (lnk.classList.contains(oldTheme))
			lnk.classList.remove(oldTheme);
		lnk.classList.add(newTheme);
	}
}
/*---- set theme on page --------------------------------*/

function setTheme() {

	var themeState = window.localStorage.getItem("theme");

	if (themeState == "lightTheme") {
		setBodyTheme('lighttheme');
		setLinkTheme('linklighttheme');
		//select radio button
		document.getElementById("lighttheme").checked = true;
		return;
	} else if (themeState == "darkTheme") {
		setBodyTheme('darktheme');
		setLinkTheme('linkdarktheme');
		//select radio button
		document.getElementById("darktheme").checked = true;
		return;
	} else {
		//select radio button
		document.getElementById("pagetheme").checked = true;
		let testdark = document.getElementsByClassName('darktheme')[0];
		if (testdark) {
			setBodyTheme('darktheme');
			setLinkTheme('linkdarktheme');
			return;
		}
		let testlight = document.getElementsByClassName('lighttheme')[0];
		if (testlight) {
			setBodyTheme('lighttheme');
			setLinkTheme('linklighttheme');
			return;
		}
	}
	/*-- unthemed --*/
}
/*---- change theme on page -----------------------------*/

function toggleTheme() {
	var elem = document.getElementsByTagName('body')[0];
	var radios = document.getElementsByName('group1');

	if (radios[0].checked) {
		window.localStorage.setItem("theme", "lightTheme");
	} else if (radios[1].checked) {
		window.localStorage.setItem("theme", "darkTheme");
	} else if (radios[2].checked) {
		window.localStorage.setItem("theme", "pageTheme");
	}
	// Reload the page on click of the radio button
	location.reload();
}

function generateDate() {
	n = new Date();
	y = n.getFullYear();
	m = n.getMonth() + 1;
	d = n.getDate();
	document.getElementById("date").innerHTML = m + "/" + d + "/" + y;
}
//----< toggle nav keys display >------------------------------------

function toggleNavKeys() {
	var nkc = document.getElementsByTagName("navKeysContainer");
	var tog = window.getComputedStyle(nkc[0], null).getPropertyValue("display");
	if (tog === "none") {
		nkc[0].style.display = "inline";
		window.localStorage.setItem("navKeyState", "show");
	}
	else {
		nkc[0].style.display = "none";
		window.localStorage.setItem("navKeyState", "hide");
	}
}
//----< setNavKeys display >-----------------------------------------

function setNavKeys() {
	var nkc = document.getElementsByTagName("navKeysContainer");
	var navKeyState = window.localStorage.getItem("themeState");
	if (navKeyState === null) {
		nkc[0].style.display = "inline";
		window.localStorage.setItem("themeState", "show");
		return;
	}
	if (navKeyState === "show") {
		nkc[0].style.display = "inline";
	}
	else {
		nkc[0].style.display = "none";
	}
}