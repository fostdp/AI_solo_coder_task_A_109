/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#8B4513',
          light: '#D2691E',
          pale: '#DEB887',
          dark: '#5D2E0C',
          cream: '#F5F0E6',
          paper: '#FDF8F0'
        },
        status: {
          normal: '#2E8B57',
          warning: '#DAA520',
          alert: '#DC143C'
        }
      },
      fontFamily: {
        serif: ['Georgia', 'serif'],
        sans: ['Noto Sans SC', 'sans-serif']
      },
      animation: {
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'fade-in': 'fadeIn 0.5s ease-in-out',
        'slide-up': 'slideUp 0.3s ease-out',
        'glow': 'glow 2s ease-in-out infinite alternate'
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' }
        },
        slideUp: {
          '0%': { transform: 'translateY(10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' }
        },
        glow: {
          '0%': { boxShadow: '0 0 5px rgba(220, 20, 60, 0.5)' },
          '100%': { boxShadow: '0 0 20px rgba(220, 20, 60, 0.8), 0 0 30px rgba(220, 20, 60, 0.4)' }
        }
      }
    },
  },
  plugins: [],
}
