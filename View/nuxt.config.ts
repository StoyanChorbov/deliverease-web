// https://nuxt.com/docs/api/configuration/nuxt-config
import vuetify, {transformAssetUrls} from "vite-plugin-vuetify";
import tailwindcss from "@tailwindcss/vite";

export default defineNuxtConfig({
    compatibilityDate: '2024-11-01',
    devtools: {enabled: false},
    app: {
        baseURL: '/',
    },
    nitro: {
        publicAssets: [
            {
                dir: 'public',
                baseURL: '/'
            }
        ]
    },
    build: {
        transpile: ['vuetify'],
    },
    modules: [
        (_options, nuxt) => {
            // Add a plugin to the build
            nuxt.hooks.hook('vite:extendConfig', (config) => {
                config.plugins = config.plugins || [];
                config.plugins.push(vuetify({autoImport: true}))
            });
        },
    ],
    css: ['~/assets/css/main.css'],
    vite: {
        vue: {
            template: {
                transformAssetUrls,
            }
        },
        plugins: [
            tailwindcss(),
        ]
    },
    runtimeConfig: {
        public: {
            mapboxKey: "pk.eyJ1Ijoic2xhenkiLCJhIjoiY204NjA0NWF1MjY3cTJrc2F2b2NwdWozeCJ9.hnt0ctjDcF1VDr3TGyfldA",
        }
    }
});
