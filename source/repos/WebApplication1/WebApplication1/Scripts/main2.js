// OPEN & CLOSE CART
const cartIcon = document.querySelector("#cart-icon");
const cart = document.querySelector(".cart");
const closeCart = document.querySelector("#cart-close");

cartIcon.addEventListener("click", (e) => {
    e.preventDefault();
    cart.classList.toggle("active");
});

closeCart.addEventListener("click", () => {
    cart.classList.remove("active");
});

// ============= NOTIFICATION FUNCTION =============
function showNotification(message, type = 'success', duration = 3000) {
    const notificationContainer = document.getElementById('notificationContainer');
    
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    
    // Choose icon based on type
    let icon = 'bx-check-circle';
    if (type === 'error') icon = 'bx-error-circle';
    if (type === 'warning') icon = 'bx-error';
    
    notification.innerHTML = `
        <i class='bx ${icon}'></i>
        <div class="notification-content">
            <div class="notification-message">${message}</div>
        </div>
    `;
    
    notificationContainer.appendChild(notification);
    
    // Trigger animation
    setTimeout(() => {
        notification.classList.add('show');
    }, 10);
    
    // Remove notification after duration
    setTimeout(() => {
        notification.classList.remove('show');
        notification.classList.add('hide');
        
        setTimeout(() => {
            notification.remove();
        }, 400);
    }, duration);
}

// ============= UPDATE CART COUNT BADGE =============
function updateCartCount() {
    const cartCountElement = document.querySelector('.cart-count');
    if (cartCountElement) {
        const totalItems = itemsAdded.length;
        cartCountElement.textContent = totalItems;
        
        // Add visual feedback when count changes
        if (totalItems > 0) {
            cartCountElement.style.display = 'flex';
            cartCountElement.classList.add('pulse');
            setTimeout(() => {
                cartCountElement.classList.remove('pulse');
            }, 300);
        } else {
            cartCountElement.style.display = 'none';
        }
    }
}

// Start when the document is ready
if (document.readyState == "loading") {
    document.addEventListener("DOMContentLoaded", start);
} else {
    start();
}

// =============== START ====================
function start() {
    addEvents();
    updateCartCount();
}

// ============= UPDATE & RERENDER ===========
function update() {
    addEvents();
    updateTotal();
    updateCartCount();
}

// =============== ADD EVENTS ===============
function addEvents() {
    // Remove items from cart
    let cartRemove_btns = document.querySelectorAll(".cart-remove");
    console.log(cartRemove_btns);
    cartRemove_btns.forEach((btn) => {
        btn.addEventListener("click", handle_removeCartItem);
    });

    // Change item quantity
    let cartQuantity_inputs = document.querySelectorAll(".cart-quantity");
    cartQuantity_inputs.forEach((input) => {
        input.addEventListener("change", handle_changeItemQuantity);
    });

    // Add item to cart
    let addCart_btns = document.querySelectorAll(".add-cart");
    addCart_btns.forEach((btn) => {
        btn.addEventListener("click", handle_addCartItem);
    });

    // Buy Order
    const buy_btn = document.querySelector(".btn-buy");
    buy_btn.addEventListener("click", handle_buyOrder);
}

// ============= HANDLE EVENTS FUNCTIONS =============
let itemsAdded = [];

function handle_addCartItem(e) {
    // Prevent default link behavior
    if (e) e.preventDefault();
    
    // Find the closest product box container
    let product = this.closest('.box') || this.closest('.product-box');
    
    if (!product) {
        console.error('Product container not found');
        return;
    }
    
    let titleElement = product.querySelector(".product-title");
    let priceElement = product.querySelector(".product-price");
    let imgElement = product.querySelector(".product-img");
    
    if (!titleElement || !priceElement || !imgElement) {
        console.error('Product elements not found', { titleElement, priceElement, imgElement });
        return;
    }
    
    let title = titleElement.innerHTML.trim();
    let price = priceElement.innerHTML;
    let imgSrc = imgElement.src;
    
    console.log('Adding to cart:', title, price, imgSrc);

    // Check if item already exists in cart
    const existingItem = itemsAdded.find((el) => el.title == title);
    
    if (existingItem) {
        // Find the cart box for this item and increase quantity
        const cartBoxes = document.querySelectorAll(".cart-box");
        cartBoxes.forEach((box) => {
            const boxTitle = box.querySelector(".cart-product-title").innerHTML;
            if (boxTitle === title) {
                const quantityInput = box.querySelector(".cart-quantity");
                let currentQty = parseInt(quantityInput.value);
                if (currentQty < 100) {
                    quantityInput.value = currentQty + 1;
                    showNotification(`Quantity updated for "${title}"!`, "success");
                } else {
                    showNotification("Maximum quantity (100) reached for this item!", "warning");
                }
            }
        });
    } else {
        // Add new item to cart
        let newToAdd = {
            title,
            price,
            imgSrc,
        };
        itemsAdded.push(newToAdd);

        // Add product to cart
        let cartBoxElement = CartBoxComponent(title, price, imgSrc);
        let newNode = document.createElement("div");
        newNode.innerHTML = cartBoxElement;
        const cartContent = cart.querySelector(".cart-content");
        cartContent.appendChild(newNode);

        // Show success notification
        showNotification(`"${title}" added to cart!`, "success");
    }

    // DO NOT automatically open the cart sidebar - only open when cart icon is clicked

    update();
}

function handle_removeCartItem() {
    const itemTitle = this.parentElement.querySelector(".cart-product-title").innerHTML;
    
    this.parentElement.remove();
    itemsAdded = itemsAdded.filter(
        (el) => el.title != itemTitle
    );

    showNotification(`"${itemTitle}" removed from cart`, "success");

    update();
}

function handle_changeItemQuantity() {
    if (isNaN(this.value) || this.value < 1) {
        this.value = 1;
    } else if (this.value > 100) {
        this.value = 100;
        showNotification("Maximum quantity allowed is 100", "warning");
    }
    
    this.value = Math.floor(this.value); // to keep it integer

    update();
}

function handle_buyOrder() {
    if (itemsAdded.length <= 0) {
        showNotification("Your cart is empty! Please add items first.", "warning");
        return;
    }
    const cartContent = cart.querySelector(".cart-content");
    cartContent.innerHTML = "";
    showNotification("Your order has been placed successfully!", "success", 4000);
    itemsAdded = [];

    update();
}

// =========== UPDATE & RERENDER FUNCTIONS =========
function updateTotal() {
    let cartBoxes = document.querySelectorAll(".cart-box");
    const totalElement = cart.querySelector(".total-price");
    let total = 0;

    cartBoxes.forEach((cartBox) => {
        let priceElement = cartBox.querySelector(".cart-price");
        let priceString = priceElement.innerHTML;
        
        // Remove currency symbol (₱ or &#8369;), commas, and any whitespace
        priceString = priceString.replace(/₱|&#8369;|,|\s/g, '');
        
        let price = parseFloat(priceString);
        let quantity = parseInt(cartBox.querySelector(".cart-quantity").value);
        
        // Only add to total if both price and quantity are valid numbers
        if (!isNaN(price) && !isNaN(quantity)) {
            total += price * quantity;
        }
    });

    // Format total with currency sign and keep 2 digits after the decimal point
    total = "₱" + total.toFixed(2);
    totalElement.innerHTML = total;
}

// ============= HTML COMPONENTS =============
function CartBoxComponent(title, price, imgSrc) {
    return `
    <div class="cart-box">
        <img src=${imgSrc} alt="" class="cart-img">
        <div class="detail-box">
            <div class="cart-product-title">${title}</div>
            <div class="cart-price">${price}</div>
            <input type="number" value="1" class="cart-quantity" style="width: 60px;"> <!-- Adjust the width as needed -->
        </div>
        <!-- REMOVE CART  -->
        <i class='bx bxs-trash-alt cart-remove'></i>
    </div>`;
}