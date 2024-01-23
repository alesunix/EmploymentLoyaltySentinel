window.downloadFile = (fileName, file) => {
    const link = document.createElement('a');
    link.href = file;
    link.download = fileName ?? '';
    link.click();
    link.remove();
}
function SaveXls(fileName, byteBase64) {
    const link = document.createElement('a');
    link.download = fileName ?? '';
    link.href = 'data:application/octet-stream;base64,' + byteBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

//function FormatDecimal(id) {
//    const input = document.getElementById(id)
//    input.addEventListener('input', function (event) {
//        let value = input.value
//        value = value.replace(/[^0-9.]/g, '')
//        value = parseFloat(value).toLocaleString('ru-RU')
//        input.value = value
//    })

//    document.addEventListener('DOMContentLoaded', () => {
//        const input = document.getElementById(id)
//        let value = input.value
//        console.log(value)
//        value = value.replace(/[^0-9.]/g, '')
//        value = parseFloat(value).toLocaleString('en-US')
//        input.value = value
//    })
//}