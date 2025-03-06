import { defineConfig, minimal2023Preset } from "@vite-pwa/assets-generator/config";
import { manifest } from "./Pages/Assets/manifest.js";

const images = ["./Pages/Assets/Static/favicon.svg"];

export default defineConfig({
    headLinkOptions: {
        preset: "2023"
    },
    preset: {
        ...minimal2023Preset,
        maskable: {
            sizes: [512],
            resizeOptions: {
                background: manifest.background_color
            }
        }
    },
    images
});
