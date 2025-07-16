const fs = require('fs')
const path = require('path')

function deleteMapFiles(dir) {
  fs.readdir(dir, (err, files) => {
    if (err) throw err

    files.forEach((file) => {
      const filePath = path.join(dir, file)
      fs.stat(filePath, (err, stat) => {
        if (err) throw err

        if (stat.isDirectory()) {
          deleteMapFiles(filePath) // Рекурсивно обходим поддиректории
        } else if (file.endsWith('.map')) {
          fs.unlink(filePath, (err) => {
            if (err) throw err
            console.log(`Deleted: ${filePath}`)
          })
        }
      })
    })
  })
}

deleteMapFiles(__dirname) // Запускаем из корневой директории проекта
