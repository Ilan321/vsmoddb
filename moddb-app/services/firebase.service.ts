// Import the functions you need from the SDKs you need
import { initializeApp } from 'firebase/app';
import { getAnalytics } from 'firebase/analytics';

const firebaseConfig = {
  apiKey: 'AIzaSyD6KdCuiN5G81HwZ7oRGgceCXD9ZKTD1hk',
  authDomain: 'vsmoddb.firebaseapp.com',
  projectId: 'vsmoddb',
  storageBucket: 'vsmoddb.firebasestorage.app',
  messagingSenderId: '365224285588',
  appId: '1:365224285588:web:e3004ef63805f4935feb81',
  measurementId: 'G-QS5VWYTNEQ'
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const analytics = getAnalytics(app);

export default {
  app,
  analytics
};
