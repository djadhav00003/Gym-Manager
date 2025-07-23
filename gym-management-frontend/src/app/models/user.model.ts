export interface User {
  id?: number;
  fullName: string;
  email: string;
  password: string;
  role: string;

  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
}


export interface Trainer {
  id: number;
  trainerName: string;
  speciality: string;
  phoneNumber: string;
  experience: number;
  gymId: number;
}


export interface Member {
  id: number;
  memberName: string;
  age: number;
  phoneNumber: string;
  email: string;
  gymId: number;
  trainerName:string;
  planName:string
}


export interface Plan {
  id: number;
  planName: string;
  durationInDays: number;
  price: number;
  gymId: number;
}


export interface Plan {
  id: number;
  planName: string;
  durationInDays: number;
  price: number;
  gymId: number;
  isPersonalTrainerAvailable: boolean; // new field
  trainerId?:number;
}
