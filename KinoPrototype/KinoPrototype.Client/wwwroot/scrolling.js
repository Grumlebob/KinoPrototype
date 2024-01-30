function adjustPosterPosition() {
    var posterImage = document.querySelector('.PosterImage');
    if (posterImage) {
        var redboxRect = posterImage.closest('.Redbox').getBoundingClientRect();
        var offset = window.pageYOffset + redboxRect.top + (redboxRect.height / 2) - (posterImage.offsetHeight / 2);
        posterImage.style.top = offset + 'px';
    }
}

window.addEventListener('scroll', adjustPosterPosition);
