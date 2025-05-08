const RightArrowIcon = ({ color,className }: { color?: string,className:string }) => {
    return (
        <svg width="800px" height="800px" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"className={className}>
<path d="M7.88675 5.68247C7.49623 5.29195 6.86307 5.29195 6.47254 5.68247L1.58509 10.5747C0.804698 11.3559 0.805008 12.6217 1.58579 13.4024L6.47615 18.2928C6.86667 18.6833 7.49984 18.6833 7.89036 18.2928C8.28089 17.9023 8.28089 17.2691 7.89036 16.8786L3.70471 12.6929C3.31419 12.3024 3.31419 11.6692 3.70472 11.2787L7.88675 7.09669C8.27728 6.70616 8.27728 6.073 7.88675 5.68247Z" fill={color}/>
        </svg>
    );
};

export default RightArrowIcon;
