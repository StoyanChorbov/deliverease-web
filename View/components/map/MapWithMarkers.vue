<script setup lang="ts">
import mapboxgl from "mapbox-gl";
import "mapbox-gl/dist/mapbox-gl.css";
import type { MapMarker } from '~/composables/delivery/types/MapMarker';

interface MapProps {
    markers: MapMarker[];
}

const props = defineProps<MapProps>();

const runtimeConfig = useRuntimeConfig();
mapboxgl.accessToken = runtimeConfig.public.mapboxKey;

const mapContainer = ref<HTMLDivElement | null>(null);

onMounted(() => {
    if (!mapContainer.value) return;

    const map = new mapboxgl.Map({
        container: mapContainer.value,
        style: 'mapbox://styles/mapbox/streets-v12',
        center: [25.257042080629198, 42.51415543970986],
        zoom: 5,
    });

    map.on("load", () => {
        map.resize();

        props.markers.forEach(({lng, lat, label}) => {
            console.log(`Adding marker at ${lng}, ${lat} with label: ${label}`);
            new mapboxgl.Marker()
                .setLngLat([lng, lat])
                .addTo(map);
        })
    })
})

</script>

<template>
    <v-container>
        <div ref="mapContainer" class="w-full h-[400px]">
            <!-- Map will be rendered here -->
        </div>
    </v-container>
</template>