const uri = 'api/Album';
let albums = [];

function getAlbums() {
    window.fetch(uri)
        .then(response => response.json())
        .then(data => window._displayAlbums(data))
        .catch(error => console.error('Unable to get albums.', error));
}

function _displayCount(albumCount) {
    const name = (window.itemCount === 1) ? 'album' : 'albums';

    document.getElementById('counter').innerText = `${window.itemCount} ${name}`;
}

function _displayAlbums(data) {
    const tBody = document.getElementById('albums');
    tBody.innerHTML = '';

    data.forEach(album => {
        let isCompleteCheckbox = document.createElement('input');

        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        let textNode = document.createTextNode(album.title);
        td1.appendChild(textNode);
    });

    albums = data;
}