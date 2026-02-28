/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  theme: {
    extend: {
      colors: {
        canvas: '#f4f6f8',
        panel: '#ffffff',
        ink: '#18222d',
        muted: '#6b7280',
        accent: '#0f766e',
        danger: '#b42318'
      },
      boxShadow: {
        card: '0 8px 24px rgba(16, 24, 40, 0.08)'
      }
    }
  },
  plugins: []
};
