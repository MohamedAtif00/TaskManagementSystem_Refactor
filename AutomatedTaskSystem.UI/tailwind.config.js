/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./src/pages/**/*.{js,ts,jsx,tsx}",
        "./src/components/**/*.{js,ts,jsx,tsx}",
        require.resolve("react-widgets/styles.css"),
    ],
    theme: {
        extend: {},
    },
    plugins: [require("react-widgets-tailwind")],
};
