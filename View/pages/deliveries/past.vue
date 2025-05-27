<script setup lang="ts">
import { ref } from 'vue';
import type { DeliveryCategory } from '~/composables/delivery/types/DeliveryCategory';
import DeliveryRow from '~/components/delivery/DeliveryRow.vue';

interface UserDeliveryDto {
	id: string;
	name: string;
	startLocationRegion: string;
	endLocationRegion: string;
	category: string;
	isFragile: boolean;
}

const deliveries = ref<UserDeliveryDto[]>([]);

onMounted(() => {
	useApi<UserDeliveryDto[]>('/deliveries/past')
		.then((res) => {
			deliveries.value = res;
		})
		.catch((error) => {
			throw new Error('Error fetching past deliveries: ', error);
		});
})
</script>
<template>
	<v-container>
		<p class="text-h2">Past deliveries</p>
		<DeliveryRow
			v-for="delivery in deliveries"
			:key="delivery.id"
			:id="delivery.id"
			:deliveryName="delivery.name"
			:startLocation="delivery.startLocationRegion"
			:endLocation="delivery.endLocationRegion"
		/>
	</v-container>
</template>
