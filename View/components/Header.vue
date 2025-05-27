<script setup lang="ts">
const auth = useAuth();
const router = useRouter();

const logout = async () => {
	auth.logout();
	await router.push('/login');
};
</script>

<template>
	<header>
		<v-app-bar>
			<v-row class="flex justify-between" :class="{
				'flex-row': $vuetify.display.smAndDown,
				'flex-col': $vuetify.display.mdAndUp
			}">
				<v-col>
					<v-btn v-if="auth.isAuthenticated.value" to="/">Home</v-btn>
					<v-btn
						v-if="auth.isAuthenticated.value"
						to="/deliveries/add"
						>Add delivery</v-btn
					>
					<v-btn
						v-if="auth.isAuthenticated.value"
						to="/deliveries/find"
						>Find delivery</v-btn
					>
					<v-btn
						v-if="auth.isAuthenticated.value"
						to="/deliveries/past"
						>Past deliveries</v-btn
					>
					<v-btn v-if="!auth.isAuthenticated.value" to="/login">
						Login
					</v-btn>
					<v-btn v-if="!auth.isAuthenticated.value" to="/register">
						Register
					</v-btn>
					<v-btn
						v-if="auth.isAuthenticated.value"
						variant="plain"
						@click="logout"
					>
						Logout
					</v-btn></v-col
				>
			</v-row>
		</v-app-bar>
	</header>
</template>
