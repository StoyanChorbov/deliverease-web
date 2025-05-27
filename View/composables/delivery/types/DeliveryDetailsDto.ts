import type {LocationDto} from "~/composables/delivery/types/LocationDto";

export interface DeliveryDetailsDto {
    id: number;
    name: string;
    category: string;
    description: string;
    startingLocation: LocationDto;
    endingLocation: LocationDto;
    sender: string;
    deliverer?: string;
    recipients: string[];
}