import firebaseService from '@/services/firebase.service';

export default defineNuxtPlugin(() => {
  firebaseService.init();
});
