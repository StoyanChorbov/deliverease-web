<script setup lang="ts">
import type { DeliveryRowDto } from '~/composables/delivery/types/DeliveryRowDto';
import { useApi } from '~/composables/useApi';
import type { LocationDto } from '~/composables/delivery/types/LocationDto';
import type { VCombobox } from 'vuetify/components';
import {
	getLocationSuggestions,
	formatLocations,
} from '~/composables/delivery/LocationService';
import LocationAutofill from '~/components/delivery/LocationAutofill.vue';

const destinationInput = useTemplateRef<VCombobox | null>('destination-input');

const deliveries = ref<DeliveryRowDto[]>([]);
const startingLocationSuggestions = ref<LocationDto[]>([]);
const destinationSuggestions = ref<LocationDto[]>([]);

const formattedDestinations = computed(
	() =>
		(destinationSuggestions.value = formatLocations(
			destinationSuggestions.value
		))
);

const selectedStartingLocation = ref<
	(LocationDto & { displayText: string }) | null
>(null);
const selectedEndingLocation = ref<
	(LocationDto & { displayText: string }) | null
>(null);

const getDeliveries = async () => {
	if (
		selectedStartingLocation.value !== null &&
		selectedStartingLocation.value.region !== undefined &&
		selectedEndingLocation.value !== null &&
		selectedEndingLocation.value.region !== undefined
	) {
		await useApi<DeliveryRowDto[]>(`/deliveries/options/${selectedStartingLocation.value.region}/${selectedEndingLocation.value.region}`)
			.then((res) => {
				deliveries.value = res;
			})
			.catch((err) => {
				console.error('Error fetching deliveries:', err);
			});
	}
};
</script>

<template>
	<v-container>
		<LocationAutofill
			v-model="selectedStartingLocation"
			label="Starting Location"
			:on-update-location="getDeliveries"
		/>
		<LocationAutofill
			v-model="selectedEndingLocation"
			label="Ending Location"
			:on-update-location="getDeliveries"
		/>
		<v-row>
			<v-col v-for="delivery in deliveries" :key="delivery.id">
				<v-card>
					<v-card-title
						>{{ delivery.name }} ({{
							delivery.category
						}})</v-card-title
					>
					<v-card-text>
						<p>
							Category:
							{{ delivery.startingLocationDto?.place }} ->
							{{ delivery.endingLocationDto?.place }}
						</p>
						<LazyNuxtLink :to="`/deliveries/${delivery.id}`">
							<v-btn color="primary">View Details</v-btn>
						</LazyNuxtLink>
					</v-card-text>
				</v-card>
			</v-col>
		</v-row>
	</v-container>
</template>
