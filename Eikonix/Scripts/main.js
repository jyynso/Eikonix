//let search = document.querySelector('.search-box');

//document.querySelector('#search-icon').onclick = () => {
//    search.classList.toggle('active');
//    navbar.classList.remove('active');
//}

//let navbar = document.querySelector('.navbar');

//document.querySelector('#menu-icon').onclick = () => {
//    navbar.classList.toggle('active');
//    search.classList.remove('active');
//}

// Mobile menu toggle
let navbar = document.querySelector('.navbar');
let menuIcon = document.querySelector('#menu-icon');

if (menuIcon && navbar) {
    menuIcon.onclick = () => {
        navbar.classList.toggle('active');
    };
}

// Header shadow on scroll
let header = document.querySelector('header');

window.addEventListener('scroll', () => {
    if (header) {
        header.classList.toggle('shadow', window.scrollY > 0);
    }
});

// Close mobile menu on scroll
window.onscroll = () => {
    if (navbar) {
        navbar.classList.remove('active');
    }
}

// Gallery scroll functionality
let scrollContainer = document.querySelector(".gallery");
let backBtn = document.getElementById("backBtn");
let nextBtn = document.getElementById("nextBtn");

if (scrollContainer) {
    let scrollSpeed = 50;
    
    scrollContainer.addEventListener("wheel", (evt) => {
        evt.preventDefault();
        scrollContainer.scrollLeft += evt.deltaY * scrollSpeed;
    });
}

if (nextBtn && scrollContainer) {
    nextBtn.addEventListener("click", () => {
        let buttonScrollSpeed = 100;
        scrollContainer.scrollBy({
            left: buttonScrollSpeed,
            behavior: 'smooth'
        });
    });
}

if (backBtn && scrollContainer) {
    backBtn.addEventListener("click", () => {
        let buttonScrollSpeed = -100;
        scrollContainer.scrollBy({
            left: buttonScrollSpeed,
            behavior: 'smooth'
        });
    });
}