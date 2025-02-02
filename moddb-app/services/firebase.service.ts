// Import the functions you need from the SDKs you need
import { initializeApp, type FirebaseApp } from 'firebase/app';
import { getAnalytics, type Analytics } from 'firebase/analytics';

const firebaseConfig = {
  apiKey: 'AIzaSyD6KdCuiN5G81HwZ7oRGgceCXD9ZKTD1hk',
  authDomain: 'vsmoddb.firebaseapp.com',
  projectId: 'vsmoddb',
  storageBucket: 'vsmoddb.firebasestorage.app',
  messagingSenderId: '365224285588',
  appId: '1:365224285588:web:e3004ef63805f4935feb81',
  measurementId: 'G-QS5VWYTNEQ'
};

const state = {
  app: null! as FirebaseApp,
  analytics: null! as Analytics,
  init() {
    this.app = initializeApp(firebaseConfig);
    this.analytics = getAnalytics(state.app);
  }
};

export default state;
