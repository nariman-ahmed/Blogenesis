/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Areas/**/*.cshtml"
  ],
  safelist: [
    'bg-green-100',
    'text-green-800',
    'border-green-400',
    'bg-red-100',
    'text-red-800',
    'border-pink-400',
  ],
  theme: {
    extend: {
      typography: {
        DEFAULT: {
          css: {
            maxWidth: 'none',
            color: '#333',
            a: {
              color: '#3182ce',
              '&:hover': {
                color: '#2c5282',
              },
            },
          },
        },
      },
    },
  },
  plugins: [
    require('@tailwindcss/typography'),
    require('@tailwindcss/forms'),
  ],
}
