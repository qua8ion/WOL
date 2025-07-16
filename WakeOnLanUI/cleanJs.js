const fs = require('fs')
const path = require('path')

function deleteJSFiles(dir) {
  fs.readdir(dir, (err, files) => {
    if (err) throw err

    files.forEach((file) => {
      const filePath = path.join(dir, file)
      fs.stat(filePath, (err, stat) => {
        if (err) throw err

        if (stat.isDirectory()) {
          deleteJSFiles(filePath) // Рекурсивно обходим поддиректории
        } else if (file.endsWith('.js')) {
          const tsFile = filePath.replace(/\.js$/, '.ts')
          if (fs.existsSync(tsFile)) {
            fs.unlink(filePath, (err) => {
              if (err) throw err
              console.log(`Deleted: ${filePath}`)
            })
          }
        }
      })
    })
  })
}

deleteJSFiles(__dirname) // Запускаем из корневой директории проекта
