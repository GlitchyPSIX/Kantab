import { defineConfig } from 'vite'
import nunjucks from '@vituum/vite-plugin-nunjucks'
import * as path from "node:path";
import * as fs from "fs";

console.log(path.resolve(process.cwd(), 'src', 'layouts'));

const removeConstructsFolder = () => {
  return {
    name: 'strip-dev-css',
    resolveId (source) {
      return source === 'virtual-module' ? source : null
    },
    renderStart (outputOptions, inputOptions) {
      const outDir = outputOptions.dir
      const cDir = path.resolve(outDir, 'constructs')
      fs.rm(cDir, { recursive: true }, () => console.log(`Deleted ${cDir}`))
    }
  }
}

const getHtmlFiles = (dir) => {
  return fs
    .readdirSync(dir, {recursive: true})
    .filter((file) => file.endsWith('.html') && !file.includes("node_modules") && !file.includes("dist")) // all HTML, no dist or node_modules
    .map((file) => path.join(dir, `${file}`));
};

const getTemplateFiles = (dir) => {
  return fs
    .readdirSync(dir, {recursive: true})
    .filter((file) => file.endsWith('.njk') && !file.includes("src")) // ignore layout file, all NJK
    .map((file) => path.join(dir, `${file}.html`));
    
};

export default defineConfig({
  build: {
    rollupOptions: {
      input: [...getHtmlFiles("./"), ...getTemplateFiles("./")],
      
      output: {
        manualChunks: {
          mathjs: ["mathjs"]
        }
      }
    }
  },
  base: "/views",
  plugins: [ nunjucks(), removeConstructsFolder()]
})